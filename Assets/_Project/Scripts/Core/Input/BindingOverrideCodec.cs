using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using System.Reflection;

namespace Necrocis
{
    public static class BindingOverrideCodec
    {
        private sealed class Override
        {
            public InputAction Action;
            public int Index;
            public string Path, Interactions, Processors;
        }

        public static string Encode(InputActionAsset asset)
        {
            string sdkJson = asset.SaveBindingOverridesAsJson();
            JObject root = string.IsNullOrEmpty(sdkJson) ? new JObject { ["bindings"] = new JArray() } : JObject.Parse(sdkJson);
            root["schemaVersion"] = 1;
            return root.ToString(Formatting.None);
        }

        // Validate completely before changing any action. Unknown GUIDs are never guessed from a key path.
        public static bool TryApply(InputActionAsset asset, string json, out string warning)
        {
            warning = null;
            var overrides = new List<Override>();
            int unmatched = 0;
            try
            {
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var root = JObject.Parse(json, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
                    var seen = new HashSet<Guid>();
                    if (root.TryGetValue("schemaVersion", out JToken version)
                        && (version.Type != JTokenType.Integer || version.Value<int>() != 1))
                        throw new FormatException("지원하지 않는 입력 설정 버전입니다.");
                    if (root.TryGetValue("bindings", out JToken bindings))
                        ReadEntries(asset, bindings, null, overrides, seen, ref unmatched);
                    else if (root.Property("schemaVersion") != null)
                        throw new FormatException("입력 설정의 bindings 배열이 없습니다.");
                    else
                    {
                        // The old format wrapped one SDK override object per action name.
                        foreach (JProperty property in root.Properties())
                        {
                            if (asset.FindAction(property.Name, false) == null)
                                throw new FormatException("알 수 없는 이전 입력 액션입니다.");
                            if (property.Value.Type == JTokenType.Null
                                || property.Value.Type == JTokenType.String && property.Value.Value<string>() == string.Empty) continue;
                            if (!(property.Value is JObject value) || value["bindings"] == null)
                                throw new FormatException("이전 입력 설정의 액션 데이터가 올바르지 않습니다.");
                            ReadEntries(asset, value["bindings"], property.Name, overrides, seen, ref unmatched);
                        }
                    }
                }
            }
            catch (Exception error) when (error is JsonException || error is FormatException || error is OverflowException)
            {
                warning = "입력 설정을 읽지 못했습니다. 원문을 보존했습니다. " + error.Message;
                return false;
            }

            var enabled = asset.Select(action => (action, action.enabled)).ToArray();
            string previous = asset.SaveBindingOverridesAsJson();
            asset.Disable();
            try
            {
                asset.RemoveAllBindingOverrides();
                foreach (Override entry in overrides)
                    entry.Action.ApplyBindingOverride(entry.Index, new InputBinding {
                        overridePath = entry.Path, overrideInteractions = entry.Interactions, overrideProcessors = entry.Processors
                    });
                foreach (var state in enabled) if (state.enabled) state.action.Enable();
                if (unmatched > 0) warning = "ID를 매칭할 수 없는 입력 설정 " + unmatched + "개는 적용하지 않았습니다. 원문을 보존했습니다.";
                return true;
            }
            catch (Exception error)
            {
                asset.Disable();
                asset.RemoveAllBindingOverrides();
                if (!string.IsNullOrEmpty(previous)) asset.LoadBindingOverridesFromJson(previous);
                foreach (var state in enabled) if (state.enabled) state.action.Enable();
                warning = "입력 설정을 적용하지 못해 이전 상태를 복원했습니다. 원문을 보존했습니다. " + error.Message;
                return false;
            }
        }

        private static void ReadEntries(InputActionAsset asset, JToken token, string legacyAction,
            List<Override> entries, HashSet<Guid> seen, ref int unmatched)
        {
            if (!(token is JArray bindings)) throw new FormatException("입력 bindings는 배열이어야 합니다.");
            foreach (JToken value in bindings)
            {
                if (!(value is JObject item) || !Guid.TryParse(ReadString(item, "id"), out Guid id) || !seen.Add(id))
                    throw new FormatException("입력 binding ID가 없거나 중복되었습니다.");
                string path = ReadString(item, "path"), interactions = ReadString(item, "interactions"), processors = ReadString(item, "processors");
                int index = asset.FindBinding(new InputBinding { id = id }, out InputAction action);
                if (index < 0) { unmatched++; continue; }
                ValidateComponents(DecodeNull(interactions), InputSystem.TryGetInteraction);
                ValidateComponents(DecodeNull(processors), InputSystem.TryGetProcessor);
                string actionName = ReadString(item, "action");
                if (legacyAction != null && action.name != legacyAction
                    || !string.IsNullOrEmpty(actionName) && actionName != action.name && actionName != action.actionMap.name + "/" + action.name)
                    throw new FormatException("입력 action과 binding ID가 일치하지 않습니다.");
                entries.Add(new Override { Action = action, Index = index,
                    Path = DecodeNull(path), Interactions = DecodeNull(interactions), Processors = DecodeNull(processors) });
            }
        }

        private static string ReadString(JObject item, string name)
        {
            JToken value = item[name];
            if (value == null || value.Type == JTokenType.Null) return null;
            if (value.Type != JTokenType.String) throw new FormatException("입력 " + name + " 값은 문자열이어야 합니다.");
            return value.Value<string>();
        }

        private static void ValidateComponents(string text, Func<string, Type> lookup)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            try
            {
                foreach (NameAndParameters component in NameAndParameters.ParseMultiple(text))
                {
                    Type type = lookup(component.name);
                    if (type == null) throw new FormatException("등록되지 않은 입력 processor/interaction입니다: " + component.name);
                    foreach (NamedValue parameter in component.parameters)
                    {
                        FieldInfo field = type.GetField(parameter.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (field == null) throw new FormatException("알 수 없는 입력 매개변수입니다: " + parameter.name);
                        object value = parameter.value.ConvertTo(Type.GetTypeCode(field.FieldType)).ToObject();
                        if (value is float single && (float.IsNaN(single) || float.IsInfinity(single))
                            || value is double number && (double.IsNaN(number) || double.IsInfinity(number)))
                            throw new FormatException("입력 매개변수는 유한한 값이어야 합니다.");
                    }
                }
            }
            catch (Exception error) when (error is ArgumentException || error is OverflowException || error is InvalidCastException)
            {
                throw new FormatException("입력 processor/interaction 형식이 올바르지 않습니다.", error);
            }
        }

        private static string DecodeNull(string value) => value == "null" ? null : value;
    }
}

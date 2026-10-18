using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Necrocis
{
    // Native CanvasGroups are reused rather than duplicated. Keep their authored/busy state
    // and guard ownership when a view is synchronously closed and reopened.
    public sealed class ModalInputScope : IDisposable
    {
        private static readonly ConditionalWeakTable<CanvasGroup, ModalInputScope> owners = new ConditionalWeakTable<CanvasGroup, ModalInputScope>();
        private readonly GameObject root;
        private readonly CanvasGroup gate;
        private bool allowedInteraction, allowedRaycasts, disposed;
        private bool? interactiveState;
        private InputContextLease lease;
        private GameObject selection;
        public bool CanInteract => !disposed && lease != null && lease.CanInteract;

        public ModalInputScope(object owner, GameObject root, bool pause, Action escape = null,
            bool allowAuxiliary = true, bool blocksGameplay = true, bool consumeEscape = true)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            this.root = root;
            var canvas = root.GetComponent<Canvas>();
            if (canvas == null) canvas = root.GetComponentInParent<Canvas>();
            if (canvas == null) throw new InvalidOperationException("A visible input context needs its actual Canvas.");
            gate = root.GetComponent<CanvasGroup>();
            if (gate == null) gate = root.AddComponent<CanvasGroup>();
            if (owners.TryGetValue(gate, out ModalInputScope previous))
            {
                if (!previous.disposed) throw new InvalidOperationException("A view root already owns an input scope.");
                allowedInteraction = previous.allowedInteraction; allowedRaycasts = previous.allowedRaycasts;
                owners.Remove(gate);
            }
            else { allowedInteraction = gate.interactable; allowedRaycasts = gate.blocksRaycasts; }
            owners.Add(gate, this);
            CaptureSelection();
            try
            {
                lease = InputManager.Instance.Contexts.Register(owner, canvas.sortingOrder, pause, blocksGameplay,
                    allowAuxiliary, escape, consumeEscape, SetInteractive);
            }
            catch { disposed = true; RestoreGate(); throw; }
        }

        private bool OwnsGate => gate != null && owners.TryGetValue(gate, out ModalInputScope current) && ReferenceEquals(current, this);
        private bool Contains(GameObject target) => target != null && root != null && target.transform.IsChildOf(root.transform);
        private void CaptureSelection()
        {
            var current = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (Contains(current)) selection = current;
        }
        private void SetInteractive(bool interactive)
        {
            if (!OwnsGate || interactiveState == interactive) return;
            if (!interactive)
            {
                CaptureSelection();
                if (interactiveState == true) { allowedInteraction = gate.interactable; allowedRaycasts = gate.blocksRaycasts; }
            }
            interactiveState = interactive;
            gate.interactable = interactive && allowedInteraction;
            gate.blocksRaycasts = interactive && allowedRaycasts;
            SynchronizeSelection();
        }

        private void SynchronizeSelection()
        {
            if (!OwnsGate) return;
            var system = EventSystem.current;
            if (system == null) return;
            if (system.alreadySelecting)
            {
                var sync = root.GetComponent<ModalSelectionSync>();
                if (sync == null) sync = root.AddComponent<ModalSelectionSync>();
                sync.Schedule(SynchronizeSelection);
                return;
            }
            if (interactiveState != true)
            {
                if (Contains(system.currentSelectedGameObject)) system.SetSelectedGameObject(null);
                return;
            }
            if (selection != null && selection.activeInHierarchy)
            {
                var selectable = selection.GetComponent<Selectable>();
                if (selectable == null || selectable.IsInteractable()) system.SetSelectedGameObject(selection);
            }
            else if (!Contains(system.currentSelectedGameObject)) system.SetSelectedGameObject(null);
        }

        public void SetViewInteraction(bool interactable, bool blocksRaycasts)
        {
            if (disposed || !OwnsGate) return;
            allowedInteraction = interactable; allowedRaycasts = blocksRaycasts;
            gate.interactable = interactiveState == true && allowedInteraction;
            gate.blocksRaycasts = interactiveState == true && allowedRaycasts;
        }

        private void RestoreGate()
        {
            if (!OwnsGate) return;
            gate.interactable = allowedInteraction; gate.blocksRaycasts = allowedRaycasts;
            owners.Remove(gate);
            // Leave the native group neutral on its view; an immediate reopen must not
            // borrow a component scheduled for deferred destruction.
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            var current = lease; lease = null;
            try { current?.Dispose(); }
            finally { RestoreGate(); }
        }
    }

    internal sealed class ModalSelectionSync : MonoBehaviour
    {
        private Action pending;
        public void Schedule(Action action) => pending = action;
        private void LateUpdate()
        {
            if (pending == null || EventSystem.current != null && EventSystem.current.alreadySelecting) return;
            var action = pending; pending = null; action();
        }
        private void OnDisable() => pending = null;
    }
}

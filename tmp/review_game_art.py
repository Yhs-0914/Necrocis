from pathlib import Path
import re, json, math
from collections import Counter
from PIL import Image, ImageDraw, ImageFont

root = Path.cwd()
out = root / 'Exports/FinalBossConcepts/2026-10-15-game-matched/Review'
out.mkdir(parents=True, exist_ok=True)
guid_map = {}
for meta in (root/'Assets').rglob('*.meta'):
    try:
        txt=meta.read_text(encoding='utf-8-sig')
        match=re.search(r'^guid: ([0-9a-f]{32})', txt, re.M)
        if match: guid_map[match.group(1)]=Path(str(meta)[:-5])
    except (OSError, UnicodeError): pass
build=(root/'ProjectSettings/EditorBuildSettings.asset').read_text(encoding='utf-8-sig')
seeds=[root/p for p in re.findall(r'path: (.+\.unity)',build)]
seeds+=list((root/'Assets/_Project/Resources').rglob('*'))
text_ext={'.unity','.prefab','.asset','.mat','.anim','.controller','.overridecontroller','.playable'}
image_ext={'.png','.jpg','.jpeg','.tga','.psd'}
queue=[p for p in seeds if p.suffix in text_ext|image_ext]
seen=set(); active=set(); sources={}
while queue:
    path=queue.pop()
    if path in seen or not path.is_file(): continue
    seen.add(path)
    if path.suffix.lower() in image_ext:
        active.add(path); continue
    if path.suffix.lower() not in text_ext: continue
    try: txt=path.read_text(encoding='utf-8-sig')
    except (OSError,UnicodeError): continue
    for guid in set(re.findall(r'guid: ([0-9a-f]{32})',txt)):
        dep=guid_map.get(guid)
        if dep:
            sources.setdefault(str(dep.relative_to(root)).replace('\\','/'),[]).append(str(path.relative_to(root)).replace('\\','/'))
            queue.append(dep)
# Runtime-loaded final boss resources are included as explicit roots above.
paths=sorted(active)
manifest=[{'image':str(p.relative_to(root)).replace('\\','/'),'referenced_by':sources.get(str(p.relative_to(root)).replace('\\','/'),[])} for p in paths]
(out/'active-image-manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
try: font=ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',12)
except OSError: font=ImageFont.load_default()
try: title_font=ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',22)
except OSError: title_font=font
groups={}
for p in paths:
    s=str(p.relative_to(root)).replace('\\','/')
    if '/FinalBoss/' in s: group='FinalBoss'
    elif '/middle boss/' in s: group='MiddleBosses'
    elif '/Player/' in s: group='Players'
    elif any(x in s for x in ['/enemy/','/Enemies/','/BiomeElites/']): group='Enemies'
    elif any(x in s for x in ['/UI/','/Item/','/Skill/','/AttackVisuals/','/VFX/','/Matthew Guz/','/ItemEffects/']): group='UI_Items_VFX'
    else: group='World_Environment'
    groups.setdefault(group,[]).append(p)
for group,items in sorted(groups.items()):
    for page,start in enumerate(range(0,len(items),48),1):
        batch=items[start:start+48]
        sheet=Image.new('RGB',(1440,70+math.ceil(len(batch)/6)*240),(30,32,37))
        draw=ImageDraw.Draw(sheet)
        draw.text((18,18),f'{group} | {page} | files {start+1}-{start+len(batch)} / {len(items)}',font=title_font,fill=(240,240,240))
        for i,p in enumerate(batch):
            x=(i%6)*240; y=70+(i//6)*240
            draw.rectangle((x+3,y+3,x+236,y+195),fill=(63,62,65))
            try:
                im=Image.open(p).convert('RGBA')
                im.thumbnail((228,180),Image.Resampling.LANCZOS)
                sheet.paste(im,(x+120-im.width//2,y+96-im.height//2),im)
                # If source is an atlas, inset a first animation frame for detail.
                meta=Path(str(p)+'.meta')
                if meta.exists():
                    mt=meta.read_text(encoding='utf-8-sig')
                    rect=re.search(r'\n\s+rect:\n\s+serializedVersion: \d+\n\s+x: ([\d.]+)\n\s+y: ([\d.]+)\n\s+width: ([\d.]+)\n\s+height: ([\d.]+)',mt)
                    if rect:
                        a,b,w,h=map(lambda v:int(float(v)),rect.groups())
                        src=Image.open(p).convert('RGBA')
                        frame=src.crop((a,src.height-b-h,a+w,src.height-b))
                        frame.thumbnail((90,90),Image.Resampling.LANCZOS)
                        draw.rectangle((x+143,y+103,x+236,y+195),fill=(36,36,39))
                        sheet.paste(frame,(x+189-frame.width//2,y+149-frame.height//2),frame)
            except Exception as exc: draw.text((x+8,y+80),str(exc)[:25],font=font,fill='red')
            label=p.name
            parent=p.parent.name
            draw.text((x+7,y+201),label[:30],font=font,fill=(245,245,245))
            draw.text((x+7,y+220),parent[:30],font=font,fill=(175,180,190))
        sheet.save(out/f'{group}-{page:02}.jpg',quality=94)
print(json.dumps({'active_image_files':len(paths),'dependency_assets':len(seen),'groups':{k:len(v) for k,v in groups.items()},'review_sheets':[str(p.relative_to(root)).replace('\\','/') for p in sorted(out.glob('*.jpg'))]},ensure_ascii=False,indent=2))

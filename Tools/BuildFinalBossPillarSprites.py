"""Prepare approved image_gen tower sheets for Unity; preserve artwork and ground registration."""
from pathlib import Path
import json
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "Assets/_Project/Art/Generated/FinalBoss/ApprovedTowerDestruction"
DESTINATION = ROOT / "Assets/_Project/Resources/FinalBoss/Pillars"
REPORT = ROOT / "Exports/FinalBossConcepts/2026-10-12-tower-destruction"


def build():
    report = {}
    for biome in ("Stomach", "Intestine", "Liver", "Lung"):
        sheet = Image.open(SOURCE / f"{biome}-break-sheet.png").convert("RGBA")
        assert sheet.width % 6 == 0 and sheet.width == sheet.height * 3
        cells, bounds = [], []
        for index in range(6):
            cell = sheet.crop((index * sheet.width // 6, 0, (index + 1) * sheet.width // 6, sheet.height))
            box = cell.getchannel("A").point(lambda a: 255 if a > 64 else 0).getbbox()
            assert box is not None
            cells.append(cell)
            bounds.append(box)
        scale = min(360 / (bounds[0][3] - bounds[0][1]), 232 / max(c.width for c in cells))
        folder = DESTINATION / "Animations" / biome
        folder.mkdir(parents=True, exist_ok=True)
        frame_bounds = []
        for index, (cell, box) in enumerate(zip(cells, bounds)):
            # Keep horizontal registration and a single scale; align only the floor contact.
            cell = cell.crop((0, box[1], cell.width, box[3]))
            size = (round(cell.width * scale), round(cell.height * scale))
            cell = cell.resize(size, Image.Resampling.NEAREST)
            frame = Image.new("RGBA", (256, 384))
            frame.alpha_composite(cell, ((256 - size[0]) // 2, 384 - size[1]))
            assert all(frame.getpixel(p)[3] == 0 for p in ((0, 0), (255, 0), (0, 383), (255, 383)))
            if index == 0:
                frame.save(DESTINATION / f"Pillar_{biome}_Intact.png")
            else:
                frame.save(folder / f"Break_{index:02d}.png")
            if index == 5:
                frame.save(DESTINATION / f"Pillar_{biome}_Broken.png")
            frame_bounds.append(frame.getchannel("A").getbbox())
        report[biome] = {"uniform_scale": scale, "frame_bounds": frame_bounds, "canvas": [256, 384], "ground_pixel": 384}
    REPORT.mkdir(parents=True, exist_ok=True)
    (REPORT / "unity-sprite-validation.json").write_text(json.dumps(report, indent=2) + "\n")
    print("PASS: four intact, four broken and twenty destruction PNGs; transparent 256x384, uniform per-tower scale, bottom-center ground anchor.")


if __name__ == "__main__":
    build()

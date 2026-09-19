"""Import original CC0 Kenney geometry, retaining source downloads and provenance."""
from pathlib import Path
import json, shutil

ROOT=Path(__file__).resolve().parents[1]
selection={
 'racing':['bannerTowerGreen','bannerTowerRed','barrierRed','barrierWhite','billboard','billboardLow','fenceStraight','flagCheckers','flagGreen','flagRed','grandStand','grandStandAwning','grandStandCovered','lightPostLarge','lightPostModern','pitsGarage','pitsOffice','pylon','radarEquipment','railDouble','tent','tentClosed','tentRoofDouble','treeLarge','treeSmall'],
 'industrial':['building-a','building-c','building-f','chimney-large','detail-tank-large','detail-tank','shipping-container-a','shipping-container-b','shipping-container-c','solar-panel-landscape-group','solar-panel-portrait-group','water-tower','windmill'],
 'nature':['cactus_short','cactus_tall','flower_purpleA','flower_redA','flower_yellowA','grass_large','grass_leafs','grass_leafsLarge','log_stack','mushroom_redGroup','mushroom_tanGroup','plant_bushDetailed','plant_bushLarge','plant_bushSmall','stone_largeA','stone_largeC','stone_smallA','stone_smallC','stone_tallA','stone_tallC']
}
# Reuse the established audited OBJ palette importer with separate aliases/output manifest.
code=(ROOT/'Tools/import_city_assets.py').read_text()
start=code.index('selection = {');end=code.index('\nout =',start)
code=code[:start]+'selection = '+repr(selection)+code[end:]
code=code.replace("ROOT/'ArtSource/ThirdParty'/pack", "ROOT/'ArtSource/ThirdParty/detail-update'/pack")
code=code.replace("alias='K_'+name", "alias='D_'+pack+'_'+name")
code=code.replace("'ArtSource/ThirdParty/import-manifest.json'", "'ArtSource/ThirdParty/detail-update/import-manifest.json'")
exec(compile(code,__file__,'exec'))

images={
 'Carbon':'5ecb7f7e-9293-4c4d-9416-13ab67c8a6cd',
 'Glitter':'53e8f556-86da-4576-9d68-0ee24edd699e',
 'Jelly':'fab2029f-3687-4794-a7c3-57e00946e062',
 'ForgedCarbon':'29cc425f-bbcb-48d2-8cb8-f42c1a716e09',
 'Pearl':'889eaf69-5152-4f97-896d-6602c3d445d0',
 'BrushedMetal':'e07bbb53-eaea-4097-b396-4f91e3c772d6',
 'Marble':'4f770c75-030f-4d3e-bea0-448528a422fd',
 'Lava':'34614f06-5eaf-4e20-a4e4-a510d0f16b28',
 'LiquidChrome':'baf088b0-a1a2-49c1-bfd0-00efb0f1aa8b',
 'Honeycomb':'bf93476d-af22-4583-ab4e-ce80010ffd73'
}
destination=ROOT/'Assets/Resources/PaintStyles';destination.mkdir(exist_ok=True)
source=Path('C:/Users/Lemon PC/.codex/generated_images/01a09464-fd28-73a2-bcda-faa329ba337e')
for name,ident in images.items():shutil.copy2(source/('exec-'+ident+'.png'),destination/(name+'.png'))
(ROOT/'ArtSource/ThirdParty/detail-update/SOURCES.md').write_text('''# Downloaded scenery, 12 September 2026

58 selected original meshes from Kenney, CC0. Original archives, OBJ geometry, MTL files, palette textures and licenses are retained here. The import manifest records source hashes, bounds and polygon counts. Runtime map detail sectors use D_ prefixed resources; no stock model is represented as newly downloaded.

- Racing Kit: https://kenney.nl/assets/racing-kit
- City Kit Industrial: https://kenney.nl/assets/city-kit-industrial
- Nature Kit: https://kenney.nl/assets/nature-kit

Paints: ten separately AI-generated grayscale automotive material textures, copied unchanged into Assets/Resources/PaintStyles. Prompt: seamless tileable automotive grayscale albedo, evenly lit flat material scan, no mockup, text or frame, to multiply by car paint color. Styles: woven carbon twill; metallic glitter flakes; cellular jelly with bubbles; forged carbon shards; nacre pearl layers; brushed aluminum scratches; marble veins; volcanic plates with bright fissures; honeycomb metal cells; liquid chrome ripples. The object-space triplanar shader wraps each texture onto body panels and painted accessories, with finish-specific reflectivity, normal relief and tint.
''')

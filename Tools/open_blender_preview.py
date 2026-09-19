import bpy
from mathutils import Quaternion
# Keep the modeled parts editable while opening on the reference-inspired car.
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type == 'VIEW_3D':
            area.spaces.active.region_3d.view_perspective='CAMERA'
            area.spaces.active.shading.type='MATERIAL'
for c in bpy.data.collections:
    if c.name in ['Comet','Miso','Nomad','Spectre']:
        c.hide_viewport=True

# Earth Coordinate Model Source

The original spheric_coords_clean.blend and its original .meta file are retained
here for editing and historical reference. This folder is deliberately outside
Assets so Unity does not require Blender on every developer's machine.

The runtime/imported model remains:
Assets/Sandbox/Simulation/Objects/Earth/spheric_coords_clean.fbx

Earth.prefab and coords.prefab now reference that existing FBX, rather than
asking Unity to convert the .blend file. Its GUID and import settings were not
changed. Unity 2022.3.62f3 verified that it supplies all 75 model file IDs used by
the two prefabs before references were changed.

Do not move the .blend back into Assets or copy its .meta onto the FBX.
When updating the art, work on a copy in Blender and export a candidate FBX
separately. Validate object names, mesh IDs, materials and prefab overrides before
replacing the production model. Keep the production FBX's .meta file.


________________________________________________________________________________________
                                     Automatic LOD
                       Copyright © 2015-2017 Ultimate Game Tools
                            http://www.ultimategametools.com
                               info@ultimategametools.com

                                         Twitter (@ugtools): https://twitter.com/ugtools
                                    Facebook: https://www.facebook.com/ultimategametools
                               Google+:https://plus.google.com/u/0/117571468436669332816
                                 Youtube: https://www.youtube.com/user/UltimateGameTools
________________________________________________________________________________________
Version 1.51


________________________________________________________________________________________
Introduction

Automatic LOD is a powerful Unity extension that allows you to quickly generate and
manage multiple levels of detail for your 3D models.
LOD levels are simplified meshes that are generated procedurally by the extension and
help optimize your game especially on lower end platforms.
The Automatic LOD component takes care at runtime of enabling the correct mesh for each
object depending on its distance to the camera or its screen area covered.
This way you can use meshes with lower polycount when the object is far away or covers
small screen space and higher polycounts for up-close views.
Use detail only when you need it!

Features:
-Generate LOD level meshes fully procedurally with just one click
-Supports both static and skinned meshes
-Supports Unity's own LODGroup component
-LODs are automatically handled by the component. No scripting needed
-Build on top of our Mesh Simplify extension which is also included
-Includes full source code
-Includes high quality 3D models and sample scenes seen on the screenshots
-Clean, easy to use and powerful UI with multiediting support
-Valid for all platforms! Especially useful on mobile
-Supports complex object hierarchies with sub-objects and multiple materials
-Live preview both in edit mode and play mode
-Supports both screen coverage and distance to camera algorithms
-Allows finetuning assigning priorities to vertices using volumes
-Classic mesh decimation/polygon count reduction using the included Mesh Simplify tool
-VR ready!


________________________________________________________________________________________
Requirements

Unity 5.1.1 or above


________________________________________________________________________________________
Help

For up to date help: http://www.ultimategametools.com/products/automatic_lod/help
For additional support contact us at http://www.ultimategametools.com/contact


________________________________________________________________________________________
Acknowledgements

-3D Models especially developed by:
    Simon Remis (http://http://www.simonremis.com)
    Luis Santander (http://www.luissantanderart.com)
    Matías Baena (http://matiasbaena.wordpress.com)

	 
________________________________________________________________________________________
Version history

V1.51 - 10/08/2017:

[FIX] Quick fix removing some dlls that were included in version 1.50 by mistake.
      Check version 1.50 changelog for the actual new features.

[FIX] Fixed a bug that showed polygonsoup on skinned meshrenderers at runtime.

V1.50 - 28/07/2017:

[FIX] Fixed a bug that showed polygonsoup on skinned meshrenderers at runtime.

      Technical details:
      There is a bug in Unity where its 3D engine doesn't like switching between
      meshes during OnWillRenderObject(). This needs to be fixed on Unity's side.
      Until it gets fixed AutomaticLOD will switch meshes/objects during Update(),
      which means that if you use Switch Mesh/Switch GameObject there will be no
      support for multiple cameras at the same time and the last one used to render
      the scene will be used.
      If you want to override which camera will be used to activate the appropiate
      LOD level you can use the static property AutomaticLOD.UserDefinedLODCamera.
      We recommend to use the new LODGroup switch mode instead.

[FIX] Fixed preprocessor to allow for Hololens build.

[ADD] Added support for the new LODGroup switch mode.
      This integrates AutomaticLOD with Unity's native LOD system by creating a
	  LODGroup component with all their meshes and levels set up automatically!

      Known issues:
      Scripting support for LODGroup is currently broken and needs fixing from Unity.
      Luckily the only limitation is the number of LOD levels is fixed to 4
      (3 + fully culled). We will try to have Unity fix it and hopefully have
      support for a variable number of LOD levels soon.

V1.07 - 11/04/2017:

[FIX] Fixed a bug that had broken the athlete prefab
[FIX] Fixed all new compiler warnings

V1.06 - 11/08/2016:

[FIX] Fixed a bug that gave a call stack error when using the new Switch GameObject
      mode with static meshes with child nodes.

V1.05 - 02/08/2016:

[NEW] Big optimization for scenes with many LOD objects. Now the LOD level checks
      are distributed uniformly over time to avoid using much CPU.

V1.04 - 10/06/2016:

[NEW] New Switch Mode option added when generating LOD levels. Now you can select
      between 2 different modes:
	     -Switch Mesh: (Which was the one used by default before v1.04). Each LOD level
		  is an internal mesh that is switched on the Mesh Filter dynamically.
		  This currently has the disadvantage of randomly showing a polygonsoup mostly
		  on skinned meshes due to a Unity bug in some very rare cases.
		 -Switch GameObject: Each LOD level is a new child object and switching between
		  LOD levels is done by enabling/disabling each GameObject accordingly.
		  This addresses the polygonsoup bug.        

[FIX] Objects using 100% of the source vertices for any LOD now have less memory usage
      because no mesh copy is being made.

V1.03 - 31/05/2016:

[FIX] Objects using 100% of the source vertices now use the original mesh and will
      not have wrong normals.  

V1.02 - 04/02/2016:

[FIX] Fixed compiler deprecated warnings for newer versions of Unity 5.

V1.01 - 16/08/2015:

[FIX] Object-camera distance is now computed using the renderer bounding box instead
      of the object's pivot. This gives better results in objects that have the pivot
	  not placed properly.

[FIX] LOD child nodes now switch between the same LODs as the parent root even if
      the "Override xxx settings" has been activated. This gives smoother results.

[FIX] Sample scene #2 now has Torii sharing the same LOD settings. Previous versions
      had different settings which could create confusion as LODs would pop at
	  different distances and different resolutions.
	  
V1.00 - 29/07/2015:

[---] Initial release

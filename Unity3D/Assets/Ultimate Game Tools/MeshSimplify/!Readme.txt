________________________________________________________________________________________
                                     Mesh Simplify
                        Copyright © 2015-2017 Ultimate Game Tools
                            http://www.ultimategametools.com
                               info@ultimategametools.com

                                         Twitter (@ugtools): https://twitter.com/ugtools
                                    Facebook: https://www.facebook.com/ultimategametools
                               Google+:https://plus.google.com/u/0/117571468436669332816
                                 Youtube: https://www.youtube.com/user/UltimateGameTools
________________________________________________________________________________________
Version 1.10


________________________________________________________________________________________
Introduction

Mesh Simplify is a powerful Unity extension that allows you to quickly reduce polygon
count on your 3D models using just a single click.
Mesh simplification is especially useful when importing high resolution meshes or
targeting lower end mobile platforms where low polycount is key.
Our Automatic LOD package includes Mesh Simplify in its full extension, so if LOD
management is required please consider getting our Automatic LOD package instead.

Features:
-Simplify / decimate meshes procedurally using just one click
-Finetune mesh simplification using different parameters
-Select mesh areas that should have less priority or more priority during polygoncount
 reduction.
-Supports both static and skinned meshes!
-Includes full source code
-Includes high quality 3D models and sample scenes seen on the screenshots
-Clean, easy to use and powerful UI
-Valid for all platforms! Especially useful on mobile
-Supports complex object hierarchies with sub-objects and multiple materials

Mesh optimization made easy!


________________________________________________________________________________________
Requirements

Unity 5.1 or above


________________________________________________________________________________________
Help

For up to date help: http://www.ultimategametools.com/products/mesh_simplify/help
For additional support contact us at http://www.ultimategametools.com/contact


________________________________________________________________________________________
Acknowledgements

-3D Models especially developed by:
    Simon Remis (http://www.simonremis.com/)
    Luis Santander (http://www.luissantanderart.com/)
    Matías Baena (https://matiasbaena.wordpress.com/)

	 
________________________________________________________________________________________
Version history

V1.10 - 28/07/2017:

[FIX] Fixed bug that did not save mesh assets to disk when "Enable Prefab Usage" was
      checked before performing any mesh simplification operation on the object.
[ADD] Added RuntimeMeshSimplifier script for runtime mesh simplification
[ADD] Added PlayMaker actions UnityPackage. For now added support for runtime
	  mesh simplification.

V1.02 - 11/04/2017:

[FIX] Fixed Sammy model which did have an incorrect simplified hierarchy.
[FIX] Fixed scripting obsolete warning.

V1.01 - 31/05/2016:

[FIX] Objects using 100% of the source vertices now use the original mesh and will
      not have wrong normals.  

V1.00 - 29/07/2015:

[---] Initial release
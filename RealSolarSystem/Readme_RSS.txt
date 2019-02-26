Real Solar System
NathanKell
This mod will convert the Kerbol System into the (Real) Solar System, rescaling, moving, and changing KSP's planets, moons, etc. to match our own.

Thanks to asmi for kicking me into to doing this and offering so much help along the way; to ferram for aeronautics and orbital help (and FAR, which is essential), and for countless time spent helping others get the most from this mod (and me the most from my modding); to ZRM for many ideas and info; to yargnit and MedievalNerd for playtesting extraordinaire; to everyone else who offered suggestions, code, help, cool screenshots...

Supreme thanks to regex for code, and dimonnomid and SpacedInvader for incredible art for RSS, and pingopete for his work on RSS - EVE interoperability and atmosphere work for RSS. RSS would not look or perform the way it does (or have gotten released!) without their amazing contributions.

License: CC-BY-NC-SA
Includes code by Majiir (CompatibilityChecker, licensed as per source) and stupid_chris (ConfigNodeExtensions, licensed CC-BY-NC-SA). Used with permission.
Includes artwork by dimonnomid and SpacedInvader.
Includes biomes by Felger
Some planetary imagery is derived from work by Steve Albers and NASA / Jet Propulsion Laboratory, and some from the Celestia Motherlode (itself in the main sourced from JPL). Used by permission of the licenses for non-commercial release.

Also included:
Module Manager (by sarbian, swamp_ig, and ialdabaoth). See thread for details, license, and source: http://forum.kerbalspaceprogram.com/threads/55219
Custom Biomes by Trueborn. See thread for details, license, and source: http://forum.kerbalspaceprogram.com/threads/66256
DDSLoader by Sarbian. See thread for details, license, and source: http://forum.kerbalspaceprogram.com/threads/96729
A configuration for Custom Asteriods by Starstrider42. See thread for details, license, and source: http://forum.kerbalspaceprogram.com/threads/80483


INSTALLATION:
Extract to KSP/GameData.

TEXTURE INSTALLATION
Now, you must select a texture resolution. You may download a premade pack (8192, 4096, or 2048), or pick and choose. Any planets which do not have textures will not have their coloration, features, etc., changed, although they will be made larger.
You can get the textures from: https://nabaal.net/files/ksp/nathankell/RealSolarSystem/Textures/

NOTE: You really should play with the recommended mods. See the Realism Overhaul thread for details.

List of planets:
Mercury is represented by Moho
Venus is represented by Eve
Earth is represented by Kerbin
Moon is represented by Mun
Mars is represented by Duna
Phobos is represented by Bop
Deimos is represented by Gilly
Jupiter is represented by Jool
Io is represented by Pol
Europa is represented by Eeloo
Ganymede is represented by Tylo
Callisto is represented by Ike
Saturn is represented by Dres
Titan is represented by Laythe
Uranus is represented by Minmus
Pluto is represented by Vall

===========================
Changelog
v8.6.1
* Removed clip value, things shouldn't flicker as badly.

v8.6
* Fixed Ganymede normal map being flipped (either redownload your texture pack or just get the fixed Ganymede normal map, it's the same for all packs) -- thanks sashan!
* Starwaster: fix temperature and pressure issues on some bodies due to multipliers not being reset.
* Removed keypress-based camera clipping changes. There's a GUI for that, after all...
* Fixed camera clipping range loading.
* Fixed finding AtmosphereFromGround (was only running if the body had a PQS--Jool does not).
* Added a :FOR[RSSConfig] tag, so people can :NEEDS off that, rather than RealSolarSystem (:NEEDS[RealSolarSystem] will return true no matter what config for RSS one has, even if it's just to prettify the atmosphers like in a visual pack).
* Update to Module Manager 2.5.12 and DDS Loader 1.9

v8.5
*Fix Venus/Mars colors not showing up right (well, work around it...) NOTE YOU NEED THE DDS TEXTURES FOR THIS TO WORK.
*Fix editor extents/camera fixing
*Fix KSCSwitcher with a workaround. Each time you start KSP, the first time you load your save, you wll need to switch to a different site and then switch back. After that, it works fine.
*Updated CustomBiomes, flipped biome textures for correct bioming.

v8.4
*Update to 0.90.
*Update DDSLoader to 1.8.
*Fix caching (startup times will be much lower. NOTE: YOU MUST DELETE CACHE ON CHANGIN RSS CONFIG).
*Restart main theme when RSS finishes loading (so you have an audio cue).
*Update Custom Biomes to my proprietary v1.7.1.

v8.3
*Added many new biomes from Felger (Luna, Mars/Deimos/Phobos, Venus, Jupiter and its moons, Saturn and Titan, Uranus, Pluto.
*Includes DDSLoader from Sarbian, and full DDS support.
*Allows specifying textures via GameDatabase rather than via direct loading (remove GameData/ prefix and remove extension, and make sure the image you are referencing is not under a PluginData folder)
*Update loading to include AppRoot when loading locally (thanks TriggerAu)
*Log on not finding LaunchSites.
*Add support for VertexPlanet and for useHeightMap in LandClass (thanks Starwaster)
*Support more light-shifting (can now modify Sun.AU and Sun.brightnessCurve)

v8.2.1
*Fix typo in camera clipping that made the ground flicker (at least it wasn't another loader bug).
*Correct version this time in assembly info.

v8.2
*Finally fixed (I trust) all remaining issues from the loader rewrite
*Borrowed some PQSMod values from 6.4x Kerbin (kudos to Raptor831 et al)

v8.1.2
*Fixed normal map loading
*Added ability to set both far and near clip planes in cfg, added cfg support for camScaledSpace (camScaledSpaceNearClip like cam01FarClip in cfg)
*Water now no longer disappears when close (by setting cam01NearClip to 1)

v8.1.1
*Fixed stupidity where I deleted textures after loading them.

v8.1
*Completely revised loading system to use coroutines, added GUI. RSS will now load at the main menu, over a period of time to allow garbage collection to run. RAM usage should no longer spike as badly. Many thanks to stupid_chris for getting me set up with coroutines, and to Sarbian for help fixing some remaining issues. While loading may take slightly longer, you should be able to use more textures/parts, and you now get a handy GUI to track status.
*Support calling textures from GameDatabase for the scaled space textures (SSColor, SSBump). This allows use combined with Sarbian's DDSLoader.
*Added FOR[] for the LaunchSites MM patch
*Updated to Custom Biomes 1.6.7

v8.0
*Update to ModuleManager v2.5.0.
*Update to KSP 0.25.
*Removed some useless checks.

v7.4 \/
*Upgrade to ModuleManager 2.4.5.
*metaphor: add pressure/temperature curves for Jupiter.
*eggrobin: improve curves for solar panel power, Earth pressure and temperature.
*Use compatibility info, now.

v7.3 \/
*Added LightShifter (ported by regex, based on Alternis Kerbol source). You can now adjust light. See wiki for details.
*Update to Custom Biomes 1.6.6
*Now supports ModuleManager 2.3.x (Launch Sites work again)
*asmi: fixed Baikonur elevation and default azimuth
*regex: can now set PQS radius independent of CB radius
*Updated message display on change KSC location to use display name, not internal name
*Add more attempts at garbage collection during RSS load; they probably run as coroutines and therefore it doesn't help, but...
*Darkened Earth a bit (was too bright for non-EVE users(
*Upgrade to Module Manager 2.3.3

v7.2 \/
*Add camera clip distance changing to the inflight GUI (ALT+G)
*Moved to using a ModuleManager patch for Custom Asteroids, since it has some issues in .24.2. Use CustomAsteroids only if you want...
*Change the name of the Mercury heightmap in the cfg; the texture has a typo in its name, and rather than having everyone redownload textures, I merely changed the cfg to not have an i in the filename.
*Fix big bug with Watchdog (no more black skies)
*Add patch for (forthcoming) RealHeat for RSS atmospheres.
*Change Earth atmosphere and surface coloration a bit
*Add metaphor's curves for Venus, Mars, and Titan.
*Do garbage collection during RSS run (thanks Addie!). RSS should work at higher memory loads in 32bit.

v7.1  \/
*Update to Custom Biomes 1.6.4
*Give Earth's terrain more texture
*fix AFG bug.

v7.0  \/
*Scaled Space scaling/wrapping now plays nice with other mods and does not require OBJ files. Loads fast.
*regex: support changing orbitColor in Orbit nodes (as standard float RGBA color). dimonnomid adds colors for all RSS bodies.
*Fix to display the Display Name of the launch site when showing the icons on the planet in Tracking Station view.
*Fail gracefully when textures are missing.
*Include Custom Biomes and configs. Custom Biomes by Trueborn. Biome map by amo28. Only Earth supported so far.
*Include Custom Asteroids. Custom Asteroids by Starstrider42, config by SpacedInvader
*Support new images for every planet by default (will fail gracefully if not found)
*Added ability to edit and add more PQSMods, and to disable any PQSMod.
*Changed oceanColor (in Export node) to use 4-value color.
*Added ability to change the color ramp on the rim of scaled space shaders, either by specifying a file via SSRamp, or another body's by SSRampRef.
*Added ability to change specular color of scaled space shader via SSSpec.
*Refactored AtmosphereFromGround code to work better, allow changing more values, etc.
*Include new textures for all bodies thanks to SpacedInvader, dimonnomid, and the sources (see above)
*Include atmospher changes for all bodies
*Bug in initial orbital positions of all bodies is fixed. Note that craft in the SOI of the Sun will be off course, so BE CAREFUL. You may have to wait until your craft reach their destination SOIs before using v7.
*Added support for changing flight camera clipping distances (for use with EVE).
*Lowered max atmosphere altitudes to match approximate height of 1Pa dynamic pressure at 12,000m/s. Earth's is now 130km, for example.
*Compiled for 0.24.2 x64

v6.2  \/
*PQS->ScaledSpace wrapping now works and caches correctly. Wrap now defaults to false for backwards compatibility. RSS will export obj files (with extra lines using keyword t for vertex tangents) for all wrapped meshes, and import them (if they exist) instead of wrapping. NOTE: If you change any PQS settings (let alone changing RSS configs!) you MUST delete all .obj files in GameData/RealSolarSystem/PluginData! NOTE 2: If you don't already have cached meshes, and/or you delete them all, KSP WILL APPEAR TO HANG on the "Loading..." screen right before Main Menu. This is NORMAL. Let it run. It takes me about 15 minutes. After that, you'll get your usual load times.
*regex: fixed Space Center initial camera height (for when camera is under terrain)
*tons of launch sites from eggrobin!
*regex: fixes for KSCSwitcher; new icon; now saves and loads selected KSC location with your persistence file; shows descriptions; icons hidden on zoomout; can click to set map to focus on selected location.
*Update to ModuleManager 2.1.5
*tweaked Earth's horizon glow a bit

v6.1  \/
*Fixed only changing things if radius differs
*Fixed to work on non-Windows platforms (regex)
*Fixed many atmosphere and temperature curves (eggrobin)
*Fixed misc typos
*Added Moon retexture from SpacedInvader
*Added bodyName support
*Parsing changes, using stupid_chris's ConfigNodeExtensions
*Added KSCSwitcher from regex: can switch KSC to other presets in the Tracking Station
*Added many, many launch site configurations (compiled by eggrobin from the work by Captain Party, ferram, and others)
*Added new descriptions to all Celestial Bodies, thanks to TheKosmonaut/SpaceAnt
*Speedup in timewarpchecker from swamp_ig
*Added support for PQS->scaledspace wrapping (for real this time; didn't work before).
*Added support for replacing scaledspace meshes with spheres (use this for now, importing is broken)
*Added support for compressing normals (defaults to true)

v6    \/
*Changed Kerbin's heightmap, textures, and PQSMods to resemble Earth (allow replacing heightmaps, SS diffuse, and SS normal maps)
*PQSCity and MapDecalTangent can be positioned by latitude/longitude
*Modify scaledspace meshes based on PQS terrain automatically on game load
*Add many new PQSMod settings and limited add/remove support.
*Fixed orbit lines (thanks HoneyFox!)
*Added pressureCurve support with curves for Earth, Venus, and Mars (megathanks Starwaster!)
*Added temperatureCurve support (nothing here yet)
*Fixed a typo with tidally locked orbits (thanks eggrobin!)
*Converted orbits to Earth-relative inclination, to support axial tilt (megathanks eggrobbin!)
*Recompiled for .23.5

v5.6  \/
*Fixed wavelenght color parsing to take four arguments (colors are RGBA after all)
*Added compatibility patches for VisualEnhancements and (thanks, jrandom!) SCANSat
 
v5.5  \/
*Fixed for KSP 0.23

v5.4  \/
*Solar panels finally fixed (really!) thanks to StarWaster.
*Fixed atmo editor keychange

v5.3  \/
*Changed atmo editor to ALT-G
*Fixed Mean Anomaly to be Mean Anomaly at Epoch, fixed handling of it to actually work
*Added new root-of-config property Epoch, for setting starting date. Now, KSP year 0 is 1950.
*Support enabling/disabling atmosphere
*Used metaphor's planet config changes
*Added ability to change science parameters for all bodies (includes config by Medieval Nerd)
*Fixed SOI not respecting minimums

v5.2  \/
*Fixed Titan's atmosphere
*Fixed solar panels (I hope)
*Added configurable atmospheres per Starwaster's work.

v5.1  \/
*Fixed bug where scaledspace meshes were being faded too early (now missing planets appear)
*Fixed Duna's atmosphere end height in the info box.

v5 -- \/
*Every KSP body is now a proxy for a body in the real solar system. However, only the terrains of Kerbin and Mun are edited; the others are merely rescaled.
*Added configurable timewarp. By default 5 and 50 are removed and instead 1 million x and 6m x are added on at the end.
*Edited terrain generator for Kerbin and Mun.
*More support for parameters in the config file.
*Using Majiir's KSPAddonFixed.
*Moved KSC to be nearer the shore.

v4 -- \/
*Switched to new config file system
*Fixed max zoomout on map camera
*Fixed some scaledspace issues
*Changed VAB/SPH camera for more freedom (with a fix: thanks, asmi!)
*Removed 5x and 50x timewarp; added 1 million x and 10m ex timewarp.
*Adjusted solar panels to inverse-square law

v3 -- \/
*Automatically multiplies distances in solar panels' powerCurves by 11 so that they yield rated charge.
*Fixed Kerbin's SMA

v2.1 -- \/
*Went back to scaling Mun's scaledspace transform for now
*Fixed Mun's inclination to avg vs. Earth's equator (since we can't tilt Kerbin's rot axis, we tilt what orbits it)

v2 -- \/
*Added asmi's fix to get KSC, KSC2, and Island land to show up
*Fixed Minmus period, changed Minmus inclination and made it like a captured rock.
*No longer changing Mun's scaledspace representation (it defaults to 0 anyway).

v1 -- \/
*First stable release.
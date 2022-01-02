Real Solar System
NathanKell
Github: https://github.com/KSP-RO/RealSolarSystem
This mod will convert the Kerbol System into the (Real) Solar System.

Thanks to asmi for kicking me into to doing this and offering so much help along the way; to ferram for aeronautics and orbital help (and FAR, which is essential), and for countless time spent helping others get the most from this mod (and me the most from my modding); to ZRM for many ideas and info; to yargnit and MedievalNerd for playtesting extraordinaire; to everyone else who offered suggestions, code, help, cool screenshots...

Supreme thanks to all who helped make RSS possible! regex for code, and dimonnomid and SpacedInvader for incredible art for RSS, and pingopete for his work on RSS - EVE interoperability and atmosphere work for RSS, and KillAshley for making RSS so much better (and prettier!). RSS would not look or perform the way it does (or have gotten released!) without their amazing contributions. Thanks to stratochief and grayduster and Thomas P. for wonderful help in converting to Kopernicus and adding the new bodies--with their help RSS has entered a new era!

License: CC-BY-NC-SA
Includes artwork by dimonnomid and SpacedInvader and Dr. Walther and KillAshley in addition to NathanKell.
Includes biomes by Felger and grayduster and KellanHiggins and KillAshley.
Some planetary imagery is derived from work by Steve Albers and NASA / Jet Propulsion Laboratory, and some from the Celestia Motherlode (itself in the main sourced from JPL). Used by permission of the licenses for non-commercial release.

DEPENDENCIES:
CustomBarnKit (by sarbian). See GitHub repository for details, license, and source: https://github.com/sarbian/CustomBarnKit
KSCSwitcher (by regex, NathanKell, and jbengtson). See GitHub repository for details, license, and source: https://github.com/KSP-RO/KSCSwitcher
Kopernicus (by teknoman, bryce, Thomas P., and NathanKell). See KSP forum thread for details, license, and source: https://forum.kerbalspaceprogram.com/index.php?showtopic=181547
Module Manager (by sarbian, swamp_ig, and ialdabaoth). See KSP forum thread for details, license, and source: https://forum.kerbalspaceprogram.com/index.php?showtopic=50533

INSTALLATION:
Install the above dependencies, as per the instructions of each one, to KSP/GameData.
Extract RealSolarSystem to KSP/GameData. You should now have one .dll (Module Manager) in the root of GameData, and two folders: Kopernicus and RealSolarSystem. However, you are NOT DONE YET.

TEXTURE INSTALLATION:
Now, you must select a texture resolution. Download a premade pack (8192, 4096, or 2048) and then, if desired, selectively replace with different-resolution textures. Note that 8192 is dangerous, since you may easily run out of memory.

You can get the textures from: https://github.com/KSP-RO/ScaledRSS-Textures
Go to the releases page and grab one of the resolution packs, then (optionally) get replacements from the repository itself).
NOTE: The folder is "RSS-Textures"

SUGGESTED MODS:
RealSolarSystem ships with configurations for many other mods:

- Custom Asteroids by Starstrider42. See thread for details, license, and source: https://forum.kerbalspaceprogram.com/index.php?showtopic=72785
- Ferram Aerospace Research by ferram4. See thread for details, license, and source: https://forum.kerbalspaceprogram.com/index.php?showtopic=19321
- Not In My BackYard by magico13, and LinuxGuruGamer. See thread for details, license, and source: https://forum.kerbalspaceprogram.com/index.php?showtopic=178484
- PlanetShine by Valerian, and Papa_Joe. See thread for details, license, and source: https://forum.kerbalspaceprogram.com/index.php?/showtopic=173138
- TextureReplacer by shaw. See thread for details, license, and source: https://forum.kerbalspaceprogram.com/index.php?showtopic=96851

FINAL NOTES:
You really should play with the recommended mods. See the Realism Overhaul thread for details.

===========================
Changelog
v18.5.0
* Add Russian localization, @eggrobin

Changelog
v18.4.0
* Fix clip planes for OpenGL, @siimav

Changelog
v18.3.0
* Fix too low clip plane value which caused artifacts to appear near the horizon, @siimav
* Fix KSC runway becoming bumpy if KK is installed, @NathanKell

v18.2.0
* Fix wrong atmospheric pressure being shown in CelestialBody KBApp @NathanKell
* Set default surface/orbital navball transition altitude for all bodies @al2me6
* French body names by @eggrobin in #244
* Fix the bad tangent in Venus pressureCurve @siimav
* Add comets to Custom Asteroids config by @Starstrider42 in #226

v18.1.5
* Support KSCSwitcher's dynamic grass color (#242), @Standecco
* Use 3D orbits when Kopernicus supports this, @siimav
* Add Churchill Rocket Research Range launch site (#218) @lpgagnon

v18.1.4
* Fix the Icy Lunar surface, @siimav
* Create RationalResources.cfg, @DRVeyl
* Add space low altitude (#229), @RCrockford
* Add Mahia LaunchSiteTrackingStation (#230), @RCrockford
* Fix some vesta biome colors (#221), @lpgagnon

Changelog
v18.1.3
* Make the dishes that hover over the KSC invisible, @siimav
* Port 16 bit heightmap PQSMod from Kopernicus to RSS, @RCrockford
* Add support for more detailed heightmaps included in RSS-Textures v18.3
* Add better coast smoothing for Earth, @RCrockford, @siimav
* Fix PQS deactivate altitude for bodies, @raidernick
* Change PQS fade in and out values to match RSSVE deactivate values for these bodies, @raidernick
* Add better-looking terrain textures for all bodies, @RCrockford, @siimav, @Standecco
    - Thanks to @Gameslinx for letting us use the textures from Beyond Home
    - Thanks to KSRSS developers for letting us use their textures
* Regenerated ribbon images to clean up some fuzzy edges, @Kerbas-ad-astra
* Add zh-cn localization, @tinygrox
* Update the thermal and aerofx fade in speeds to be more realisitic, so that flame effects do not appear at mach 3, @raidernick
* Improve planetshine configs, @nepphhh
* Add additional Mars biomes, @RCrockford
* Fix many launch site PQS heights/smoothing, @raidernick
* Add White Sands and Vostochny launch sites, @raidernick
* Add Bermuda MSFN station for RA, @RCrockford
* Ensure that the file extensions for all dds files are in lowercase, @siimav
* Fix SCANsat MM patches, @Capkirk123
* Add config for Kerbal Wind, @RCrockford

v18.0
* Recompiled for KSP 1.8.1
* png textures to dds, @jrodrigv
* Implement settings for Runway Fix and disable debug info by default, @siimav 
* Match KSC grass color, @Standecco
* Additional tracking stations for RealAntennas @RCrockford

v16.4
* Integrated the RSS Runway fix from @whale2
* Removed Version Checker

v16.3
* Recompiled for KSP 1.7.X

v16.2 HOTFIX
* Wrong Assembly Version was included in file, needed a recompile with the proper information

v16.1
* Fixed the issue where KSP log stated there was no preset high for PQS (Thanks to PhineasFreak for telling me how to fix - Issue #167)
* When KSP is reading biome maps, there are imperfections for some reason. There will by 1+ pixel readings of a different color than something identified in the file. In this situation, it defaults to the first biome in the list. Re-ordered the biomes so that the most common biome is listed first to stop something like Olympus Mons showing in many places on Mars. (thanks to Faptown - Issue #163)

v16.0
* Updated for KSP 1.6.1.
* Added recovery zone support for the NIMBY (Not In My BackYard) mod (courtesy of Kerbas-ad-astra - PR #155).
* Added the Mahia launch site in New Zealand (courtesy of leudaimon - PR #157).
* Added an installation checker to verify that the required RSS textures are installed upon game startup.
* Added support for CommNet stations mirroring the existing ones by RemoteTech.
* Enabled the "On Demand" feature of Kopernicus for up to 35% reduction of memory requirements (courtesy of pap1723 - Commit b1b8507).
* Fixed the incorrect latitude of the MSFN Indian Ocean Ship tracking station (courtesy of Kerbas-ad-astra - PR #150).
* Fixed the missing surface tiles on sub-orbital flights (courtesy of siimav - PR #159).
* Fixed the incorrect thermal radiation properties of Venus, Jupiter, Saturn, Uranus and Neptune. Vessels will now explode less when
  in close proximity to these bodies (courtesy of Starwaster - PR #162).
* Fixed the Mars atmosphere fade effect.
* Fixed the Titan atmosphere color and fade effect.
* Removed a lot of non-working stock KSP PQS mods.
* Removed the stock KSP launch sites (managed for RSS by KSCSwitcher).
* Removed the compatibility patches for AntennaRange, Better Buoyancy and TextureReplacerReplaced, as these mods are now deprecated
  and/or not maintained.

v14.0
* Updated for KSP 1.4.5.
* Added a patch to fix the density and size of the asteroids.
* Cleaned up the RSS source to remove obsolete code.

v13.1
* Update custom asteroids compatibility for 1.3+.
* Fix earth terrain noise in prep for changes to ksp 1.4.x, still works in 1.3.1.
* Fixed the problem with the normal maps for Ceres and Vesta, requires update of rss textures pack.

Thanks to Starstrider42, Kerbas-ad-astra and PhineasFreak for these fixes.

v13.0
Release in the absence of NathanKell, coordinated by Raidernick.
This release moves on to KSP 1.3.1, and adds seven new celestial bodies.

Special thanks to:
- Bornholio for both individual contributions to testing and for coordinating
  testing through the Golden Spreadsheet;
- awang for promptly upgrading many RO mods, including RSS, following KSP
  releases.
Changes, in the order of pull requests:
* soundnfury, in #103: changed the longitude of the ascending node from the
  unintentional arbitrary value of 0 to another arbitrary value (any fixed value
  is arbitrary, since this value precesses with a period of about 18.6 years).
  This has no effect for users of Principia.
* Kerbas-ad-astra, in #104: some antenna upgrade patches.
* PhineasFreak, in #108: Kopernicus compatibility fixes.
* leudaimon, in #110: added downrange tracking stations for Kourou launches,
  namely Galliot, Natal, Ascension, and Libreville; renamed Malindi.
* rsparkyc, in #112: worked around #111, workaround reverted since the
  underlying Principia issue mockingbirdnest/Principia#1413 was fixed in Chasles
  (reverted in #120).
* Pap, in #113: reworked biomes and science, added Ceres, Vesta, and five moons
  of Herschel (Ariel, Miranda, Oberon, Titania, and Umbriel).
* ppboyle, in #115: fixed the Sohae Satellite Launching Station (서해위성발사장)
  which was underground.
* Kerbas-ad-astra, in #116: added ribbons for the bodies added in #113.
* Benew, in #119: fixed a typo in a biome name on the Moon.
* eggrobin, in #120: reverted #112.
* awang, in #121: Upgraded to 1.3.1.
* Kerbas-ad-astra, in #123: fixed an error in the ResourceType of Jupiter's
  resources.
* PhineasFreak, in #125: fixed the atmospheric pressure of Triton, which was off
  by an order of magnitude (#118).
* PhineasFreak, in #130: fixed ocean buoyancy.

v12.0
* HUGE THANK YOU to everyone involved! I was away and I come back to...many great changes and a pretty much ready-to-release update!
* Recompile for KSP 1.2.2, include latest MM and Kopernicus.
* Sigma88: Remove an unneeded compatibility setting.
* Raidernick: Fix Earth SMA (fixes orbital shift over time with wrong period).
* SirKeplan: More reasonable timewarp limits on Phobos and Deimos.
* Kerbas-ad-astra: Ribbon support.
* ThreePounds: Fix some Chinese launch sites, add Wenchang.
* Kerbas-ad-astra: Add atmospheric composition to Pluto.
* Miki-g: Venus warp limits fixed.
* Kerbas-ad-astra: Support (with Custom Barn Kit) updating the stock DSN ranges.
* PhineasFreak: Rework RT patching for current RemoteTech.
* Leudaimon: Fix launch site orientations for all sites.

v11.4.0
* Recompile and repackage for KSP 1.1.3.

v11.3.0
* Basic resource support thanks to Kerbas-ad-astra.
* Fix an issue with Boca thanks to BevoLJ.
* Improved oblateness thanks to Sigma88.
* Fix some rotations thanks to Sigma88.
* Fix more launch sites thanks to miki-g.
* Atmosphere improvements thanks to OhioBob.
* Fix an issue with Mars PQS thanks to Sigma88.
* Set navball to change surf<->orbit at the Karman line (requires KSP 1.1.3).
* Change inverse rotation threshold for Earth to 145km.

v11.2.0
* Fix for some missing stock textures.
* Tweaks to Mars PQS.
* Fix a typo in Tanegashima's launch site definition.
* Fix Pluto having two atmosphere nodes.
* Tweak temperature of Earth's upper atmosphere.
* Make Saturn oblate (thanks Sigma88).
* We forgot to enable some temperature curves (thanks OhioBob).
* Tweak upper atmosphere of Venus slightly (thanks OhioBob).

v11.1.0
* Update for KSP 1.1.2
* Fix a typo in an atmosphere curve preventing Sigma compatibility.
* Support for Pluton and Charon to use SigmaBinary if installed (thanks Sigma88).
* Further KillAshley material optimizations.
* Kopernicus Asteroid support thanks to PhineasFreak.
* Launch site fixes thanks to Specimen Spiff.
* Fix an issue with KSPAVC.

v11.0.0
* Update for KSP 1.1.
* Default to on demand off (x64!)
* Interoperate with Texture Replacer (helmetless on Earth).
* Force High preset for PQS.
* Use the full set of temperature curves from OhioBob now that KSP properly supports seasonal temperature changes.
* Thanks to Sigma88, RSS supports SigmaBinary for Pluto/Charon if SigmaBinary is installed.
* PQS Material fixes for 1.1 from KillAshley.

v10.6.2
* Fix packaging error (RSSKopernicus.cfg file was still included; it should not be.)

v10.6.1
* Hotfix for body indices. If you already loaded a save with 10.6, apologies.

v10.6
* Fix issue with Earth's sidereal rotation period not being correct (thanks eggrobin).
* Tweak hypersonic convection up slightly.
* **Massive improvement pass by KillAshley! Make sure you get the RSS-Textures update too! Updates:**
* Added organized cache files to local RSS folder for all bodies
* Removed RSSKopernicus.cfg; Separated & organized body cfgs one body per cfg and added RSSKopernicusSettings.cfg that holds remaining global values.
* Enforced fixed flightGolbalsIdex to all bodies to ensure future save-game compatibility
* Tweaked various planets PQS maxLevel to adjust for terrain pixelation vs detail
* Added detailed PQSMaterials to create detailed surface textures to overhauled bodies
* Added & Edited PQSMods to allow for more varied (less flat) terrain
* Added higher fidelity normal maps
* Added new heightmaps for certain bodies to improve terrain based off lack of credible data
* Adjusted scaledspace atmospheric rims to a more suitable coloring & strength
* Implemented Charon
* Added Pluto & Charon height & color maps made from current information mixed with procedurally generated terrain

v10.5
* Fixes to Russian launch sites thanks to Niemand303.
* Planetshine configs thanks to valerian.
* Support non-RP-0 science thanks to Kerbas-ad-astra and GregRoxMun.
* Rings fix thanks to Sigma88.
* Some fixes to Deimos and Phobs thanks to KillAshley and GregroxMun.
* Add back missing MSFN ground stations (as of 1963).
* North Korean launch site thanks to eggrobin.
* Fix issues with some biomes that had crept in on the transition to 1.0.
* Update atmospheric bodies' atmospheres and temperature curves thanks to OhioBob. **NOTE Earth's atmosphere now ends at 140km!**
* Refine Earth PQS/scaled space transititon to attempt to improve RVE integration.
* Titan fixes from GregroxMun.
* Atmosphere color/ramp fixes from GregroxMun.

v10.4.1
* Update physics modifiers from current RO settings.
* Update metadata.
* Update to Kopernicus 0.5.2.

v10.4
* Update for KSP 1.0.5.
* Update for Kopernicus 0.5 (thanks Thomas).
* Correct rotation of KSC (at Cape Canaveral).
* Colors for different classes of tracking stations (thanks PhineasFreak).

v10.3.1
* Un-revert reversion of Earth biomes.
* Slightly re-increase launch site comm range, to about that of a Comm16.

v10.3
* RSS itself now contains groundstation definitions for RemoteTech, with the full networks appearing when RealismOverhaul is installed (via NEEDS--the files are here). Thanks Peppie, regex!
* Patch AntennaRange antenna ranges, if that's installed. Thanks Kerbas-ad-astra!
* Update CustomAsteroids config. Thanks Ascraeus1!
* Various compatability patches thanks to Sigma88.
* Fix some typos in body descriptions, biomes. Thanks Trollception et al!
* Fix Mars scaled space fades. Thanks Raidernick!
* Fix science altitude thresholds. Thanks Laie!
* Update to Kopernicus 0.4

v10.2
* Refine AeroFX patching (when RO not around).
* Fix to not use "the" in the name of anything but the Moon.
* Remove unneeded, bad solar power curve.
* Apply some fixes to stock contracts.
* Fix typos in some biomes.
* New biome map for Earth by KellanHiggins! Thanks! You **must** update that biome texture, by downloading the RSS textures pack v10.2 or by just grabbing that texture.
* Add ocean data for Better Buoyancy compatibility.
* Fix Saturn biomes not having the correct names/colors.

v10.1
* Actually set atmospheric properties.
* Update FAR compatibility for atmospheres.
* Include physics patch on FIRST (will be overridden by anything else) to make reentries survivable etc. Also tunes down AeroFX so ascents don't look so flamey.

v10.0.2
* Recompile for KSP 1.0.4
* Default to not load if missing and to not log if missing (needs latest Kopernicus).

v10.0.1
* Fixed atmosphere shader on Neptune.
* Fixed Titan to properly grab its heightmap.
* Fixed Titan surface coloration.
* Fixed atmosphere colors.
* Fixed versioning for CKAN.

v10.0
(note: v9 was never released)
* **Utterly savebreaking.**
* Switched to Kopernicus.
* Stripped RSS plugin of anything but warp-changing and the atmosphere GUI and camera clipping.
* Added a bunch of new bodies (thanks to stratochief, grayduster, and Thomas P. for the help in conversion and body-adding).
* All names are real now.
* Added six new moons of Saturn (Enceladus, Tethys, Iapetus, Dione, Rhea, Mimas).
* Added rings to Saturn.
* Added Neptune and its moon Triton.
* Gas giant atmospheres changed such that altitude 0 = 1000 atmospheres.


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

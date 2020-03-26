/* 
 * This code is adapted from Kopernicus
 * Available from https://github.com/Kopernicus/Kopernicus
 */

using System;
using System.IO;
using DDSHeaders;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Components;
using Kopernicus.OnDemand;
using UnityEngine;
using Kopernicus.Configuration.Parsing;
using Kopernicus;

namespace RealSolarSystem
{
    /// <summary>
    /// MapSO Replacement to support Texture streaming and textures larger than 16k
    /// </summary>
    public class MapSODemandLarge : MapSO, ILoadOnDemand
    {
        // Representation of the map
        private NativeByteArray Image { get; set; }

        // States
        public Boolean IsLoaded { get; set; }
        public Boolean AutoLoad { get; set; }

        // Path of the Texture
        public String Path { get; set; }

        // Name
        String ILoadOnDemand.Name
        {
            get { return name; }
            set { name = value; }
        }

        // These textures are potentially too big to make a Texture2D of, so just load directly into the byte array
        private void LoadTexture(String path)
        {
            path = KSPUtil.ApplicationRootPath + "GameData/" + path;
            if (File.Exists(path))
            {
                try
                {
                    if (path.ToLower().EndsWith(".dds"))
                    {
                        // Borrowed from stock KSP 1.0 DDS loader (hi Mike!)
                        // Also borrowed the extra bits from Sarbian.
                        BinaryReader binaryReader = new BinaryReader(File.OpenRead(path));
                        UInt32 num = binaryReader.ReadUInt32();
                        if (num == DDSValues.uintMagic)
                        {

                            DDSHeader ddsHeader = new DDSHeader(binaryReader);

                            if (ddsHeader.ddspf.dwFourCC == DDSValues.uintDX10)
                            {
                                // ReSharper disable once ObjectCreationAsStatement
                                new DDSHeaderDX10(binaryReader);
                            }

                            Boolean alpha = (ddsHeader.ddspf.dwFlags & 0x00000002) != 0;
                            Boolean fourcc = (ddsHeader.ddspf.dwFlags & 0x00000004) != 0;
                            Boolean rgb = (ddsHeader.ddspf.dwFlags & 0x00000040) != 0;
                            Boolean alphapixel = (ddsHeader.ddspf.dwFlags & 0x00000001) != 0;
                            Boolean luminance = (ddsHeader.ddspf.dwFlags & 0x00020000) != 0;

                            Boolean mipmap = (ddsHeader.dwCaps & DDSPixelFormatCaps.MIPMAP) != 0u;
                            if (fourcc)
                            {
                                Debug.Log("[Kopernicus]: Compressed textures are not are supported for large maps.");
                            }
                            else
                            {
                                TextureFormat textureFormat = TextureFormat.ARGB32;
                                Boolean ok = true;
                                if (!rgb && alpha != luminance && (ddsHeader.ddspf.dwRGBBitCount == 8 || ddsHeader.ddspf.dwRGBBitCount == 16))
                                {
                                    if (ddsHeader.ddspf.dwRGBBitCount == 8)
                                    {
                                        // A8 format or Luminance 8
                                        if (alpha)
                                            textureFormat = TextureFormat.Alpha8;
                                        else
                                            textureFormat = TextureFormat.R8;
                                    }
                                    else if (ddsHeader.ddspf.dwRGBBitCount == 16)
                                    {
                                        // R16 format
                                        textureFormat = TextureFormat.R16;
                                    }
                                }
                                else
                                {
                                    ok = false;
                                    Debug.Log("[Kopernicus]: Only A8, R8, and R16 are supported");
                                }

                                if (ok)
                                {
                                    _name = name;
                                    _width = (Int32)ddsHeader.dwWidth;
                                    _height = (Int32)ddsHeader.dwHeight;
                                    _isCompiled = false;

                                    if (textureFormat == TextureFormat.R16)
                                        _bpp = (Int32)MapDepth.HeightAlpha;
                                    else
                                        _bpp = (Int32)MapDepth.Greyscale;

                                    _rowWidth = _width * _bpp;

                                    Int32 dataSize = _rowWidth * _height;

                                    Image = new NativeByteArray(dataSize);
                                    byte[] ddsData = binaryReader.ReadBytes(dataSize);

                                    for (Int32 i = 0; i < dataSize; i++)
                                    {
                                        Image[i] = ddsData[i];
                                    }

                                    Debug.Log("[Kopernicus]: Loaded large map: " + path);
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("[Kopernicus]: Bad DDS header.");
                        }
                    }
                    else
                    {
                        Debug.Log("[Kopernicus]: Only DDS is supported for large maps.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log("[Kopernicus]: failed to load " + path + " with exception " + ex.Message);
                }
            }
            else
            {
                Debug.Log("[Kopernicus]: texture does not exist! " + path);
            }
        }

        /// <summary>
        /// Load the Map
        /// </summary>
        public void Load()
        {
            // Check if the Map is already loaded
            if (IsLoaded)
            {
                return;
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                // Load the Map
                LoadTexture(Path);

                IsLoaded = true;

                // Create a dummy map for firing events
                MapSODemand eventMap = (MapSODemand)ScriptableObject.CreateInstance(typeof(MapSODemand));
                eventMap.name = name;
                eventMap.Path = Path;
                Events.OnMapSOLoad.Fire(eventMap);
                
                Debug.Log("[OD] ---> Map " + name + " enabling self. Path = " + Path);
                return;
            }

            // Return nothing
            Debug.Log("[OD] ERROR: Failed to load map " + name + " at path " + Path);
        }

        /// <summary>
        /// Unload the map
        /// </summary>
        public void Unload()
        {
            // We can only destroy the map, if it is loaded
            if (!IsLoaded)
            {
                return;
            }

            // Nuke the map
            if (OnDemandStorage.UseManualMemoryManagement)
            {
                Image.Free();
            }

            // Set flags
            IsLoaded = false;

            // Create a dummy map for firing events
            MapSODemand eventMap = (MapSODemand)ScriptableObject.CreateInstance(typeof(MapSODemand));
            eventMap.name = name;
            eventMap.Path = Path;
            Events.OnMapSOUnload.Fire(eventMap);

            // Log
            Debug.Log("[OD] <--- Map " + name + " disabling self. Path = " + Path);
        }

        // GetPixelByte
        public override Byte GetPixelByte(Int32 x, Int32 y)
        {
            // If we aren't loaded....
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: getting pixelbyte with unloaded map " + name + " of path " + Path + ", autoload = " + AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return 0;
                }
            }

            if (!OnDemandStorage.UseManualMemoryManagement)
            {
                return 0;
            }

            if (x < 0)
            {
                x = Width - x;
            }
            else if (x >= Width)
            {
                x -= Width;
            }

            if (y < 0)
            {
                y = Height - y;
            }
            else if (y >= Height)
            {
                y -= Height;
            }

            return Image[PixelIndex(x, y)];
        }

        // GetPixelColor - Double
        public override Color GetPixelColor(Double x, Double y)
        {
            if (IsLoaded)
            {
                return base.GetPixelColor(x, y);
            }

            if (OnDemandStorage.OnDemandLogOnMissing)
            {
                Debug.Log("[OD] ERROR: getting pixelColD with unloaded map " + name + " of path " + Path +
                          ", autoload = " + AutoLoad);
            }

            if (AutoLoad)
            {
                Load();
            }
            else
            {
                return Color.black;
            }

            return base.GetPixelColor(x, y);
        }

        // GetPixelColor - Float
        public override Color GetPixelColor(Single x, Single y)
        {
            if (IsLoaded)
            {
                return base.GetPixelColor(x, y);
            }

            if (OnDemandStorage.OnDemandLogOnMissing)
            {
                Debug.Log("[OD] ERROR: getting pixelColF with unloaded map " + name + " of path " + Path +
                          ", autoload = " + AutoLoad);
            }

            if (AutoLoad)
            {
                Load();
            }
            else
            {
                return Color.black;
            }

            return base.GetPixelColor(x, y);
        }

        // GetPixelColor - Int
        public override Color GetPixelColor(Int32 x, Int32 y)
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: getting pixelColI with unloaded map " + name + " of path " + Path +
                              ", autoload = " + AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return Color.black;
                }
            }

            if (!OnDemandStorage.UseManualMemoryManagement)
            {
                return new Color();
            }

            index = PixelIndex(x, y);
            switch (_bpp)
            {
                case 3:
                    return new Color(Byte2Float * Image[index], Byte2Float * Image[index + 1],
                        Byte2Float * Image[index + 2], 1f);
                case 4:
                    return new Color(Byte2Float * Image[index], Byte2Float * Image[index + 1],
                        Byte2Float * Image[index + 2], Byte2Float * Image[index + 3]);
            }

            if (_bpp != 2)
            {
                retVal = Byte2Float * Image[index];
                return new Color(retVal, retVal, retVal, 1f);
            }

            retVal = Byte2Float * Image[index];
            return new Color(retVal, retVal, retVal, Byte2Float * Image[index + 1]);
        }

        // GetPixelColor32 - Double
        public override Color GetPixelColor32(Double x, Double y)
        {
            if (IsLoaded)
            {
                return base.GetPixelColor32(x, y);
            }

            if (OnDemandStorage.OnDemandLogOnMissing)
            {
                Debug.Log("[OD] ERROR: getting pixelCol32D with unloaded map " + name + " of path " + Path +
                          ", autoload = " + AutoLoad);
            }

            if (AutoLoad)
            {
                Load();
            }
            else
            {
                return Color.black;
            }

            return base.GetPixelColor32(x, y);
        }

        // GetPixelColor32 - Float - Honestly Squad, why are they named GetPixelColor32, but return normal Colors instead of Color32?
        public override Color GetPixelColor32(Single x, Single y)
        {
            if (IsLoaded)
            {
                return base.GetPixelColor32(x, y);
            }

            if (OnDemandStorage.OnDemandLogOnMissing)
            {
                Debug.Log("[OD] ERROR: getting pixelCol32F with unloaded map " + name + " of path " + Path +
                          ", autoload = " + AutoLoad);
            }

            if (AutoLoad)
            {
                Load();
            }
            else
            {
                return Color.black;
            }

            return base.GetPixelColor32(x, y);
        }

        // GetPixelColor32 - Int
        public override Color32 GetPixelColor32(Int32 x, Int32 y)
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: getting pixelCol32I with unloaded map " + name + " of path " + Path +
                              ", autoload = " + AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return new Color32();
                }
            }

            if (!OnDemandStorage.UseManualMemoryManagement)
            {
                return new Color32();
            }

            index = PixelIndex(x, y);
            switch (_bpp)
            {
                case 3:
                    return new Color32(Image[index], Image[index + 1], Image[index + 2], 255);
                case 4:
                    return new Color32(Image[index], Image[index + 1], Image[index + 2], Image[index + 3]);
            }

            if (_bpp != 2)
            {
                val = Image[index];
                return new Color32(val, val, val, 255);
            }

            val = Image[index];
            return new Color32(val, val, val, Image[index + 1]);
        }

        // GetPixelFloat - Double
        public override Single GetPixelFloat(Double x, Double y)
        {
            if (IsLoaded)
            {
                return base.GetPixelFloat(x, y);
            }

            if (OnDemandStorage.OnDemandLogOnMissing)
            {
                Debug.Log("[OD] ERROR: getting pixelFloatD with unloaded map " + name + " of path " + Path +
                          ", autoload = " + AutoLoad);
            }

            if (AutoLoad)
            {
                Load();
            }
            else
            {
                return 0f;
            }

            return base.GetPixelFloat(x, y);
        }

        // GetPixelFloat - Float
        public override Single GetPixelFloat(Single x, Single y)
        {
            if (IsLoaded)
            {
                return base.GetPixelFloat(x, y);
            }

            if (OnDemandStorage.OnDemandLogOnMissing)
            {
                Debug.Log("[OD] ERROR: getting pixelFloatF with unloaded map " + name + " of path " + Path +
                          ", autoload = " + AutoLoad);
            }

            if (AutoLoad)
            {
                Load();
            }
            else
            {
                return 0f;
            }

            return base.GetPixelFloat(x, y);
        }

        // GetPixelFloat - Integer
        public override Single GetPixelFloat(Int32 x, Int32 y)
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: getting pixelFloatI with unloaded map " + name + " of path " + Path +
                              ", autoload = " + AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return 0f;
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                retVal = 0f;
                index = PixelIndex(x, y);
                for (Int32 i = 0; i < _bpp; i++)
                {
                    retVal += Image[index + i];
                }

                retVal /= _bpp;
                retVal *= Byte2Float;
                return retVal;
            }

            return 0f;
        }

        // GetPixelHeightAlpha - Double
        public override HeightAlpha GetPixelHeightAlpha(Double x, Double y)
        {
            if (IsLoaded)
            {
                return base.GetPixelHeightAlpha(x, y);
            }

            if (OnDemandStorage.OnDemandLogOnMissing)
            {
                Debug.Log("[OD] ERROR: getting pixelHeightAlphaD with unloaded map " + name + " of path " + Path +
                          ", autoload = " + AutoLoad);
            }

            if (AutoLoad)
            {
                Load();
            }
            else
            {
                return new HeightAlpha(0f, 0f);
            }

            return base.GetPixelHeightAlpha(x, y);
        }

        // GetPixelHeightAlpha - Float
        public override HeightAlpha GetPixelHeightAlpha(Single x, Single y)
        {
            if (IsLoaded)
            {
                return base.GetPixelHeightAlpha(x, y);
            }

            if (OnDemandStorage.OnDemandLogOnMissing)
            {
                Debug.Log("[OD] ERROR: getting pixelHeightAlphaF with unloaded map " + name + " of path " + Path +
                          ", autoload = " + AutoLoad);
            }

            if (AutoLoad)
            {
                Load();
            }
            else
            {
                return new HeightAlpha(0f, 0f);
            }

            return base.GetPixelHeightAlpha(x, y);
        }

        // GetPixelHeightAlpha - Int
        public override HeightAlpha GetPixelHeightAlpha(Int32 x, Int32 y)
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: getting pixelHeightAlphaI with unloaded map " + name + " of path " +
                              Path + ", autoload = " + AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return new HeightAlpha(0f, 0f);
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                index = PixelIndex(x, y);
                if (_bpp == 2)
                {
                    return new HeightAlpha(Byte2Float * Image[index], Byte2Float * Image[index + 1]);
                }

                if (_bpp != 4)
                {
                    return new HeightAlpha(Byte2Float * Image[index], 1f);
                }

                val = Image[index];
                return new HeightAlpha(Byte2Float * Image[index], Byte2Float * Image[index + 3]);
            }

            return new HeightAlpha(0f, 0f);
        }

        // GreyByte
        public override Byte GreyByte(Int32 x, Int32 y)
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: getting GreyByteI with unloaded map " + name + " of path " + Path +
                              ", autoload = " + AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return 0;
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                return Image[PixelIndex(x, y)];
            }

            return 0;
        }

        // GreyFloat
        public override Single GreyFloat(Int32 x, Int32 y)
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: getting GreyFloat with unloaded map " + name + " of path " + Path +
                              ", autoload = " + AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return 0f;
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                return Byte2Float * Image[PixelIndex(x, y)];
            }

            return 0f;
        }

        // PixelByte
        public override Byte[] PixelByte(Int32 x, Int32 y)
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: getting pixelByte with unloaded map " + name + " of path " + Path +
                              ", autoload = " + AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return new Byte[_bpp];
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                Byte[] numArray = new Byte[_bpp];
                index = PixelIndex(x, y);
                for (Int32 i = 0; i < _bpp; i++)
                {
                    numArray[i] = Image[index + i];
                }

                return numArray;
            }

            return new Byte[_bpp];
        }

        // CompileToTexture
        public override Texture2D CompileToTexture(Byte filter)
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: compiling with unloaded map " + name + " of path " + Path + ", autoload = " +
                              AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return new Texture2D(_width, _height);
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                Color32[] color32 = new Color32[Size];
                for (Int32 i = 0; i < Size; i++)
                {
                    val = (Byte)((Image[i] & filter) == 0 ? 0 : 255);
                    color32[i] = new Color32(val, val, val, 255);
                }

                Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                compiled.SetPixels32(color32);
                compiled.Apply(false, true);
                return compiled;
            }
            return new Texture2D(_width, _height);
        }

        // Generate a greyscale texture from the stored data
        public override Texture2D CompileGreyscale()
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: compiling with unloaded map " + name + " of path " + Path + ", autoload = " +
                              AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return new Texture2D(_width, _height);
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                Color32[] color32 = new Color32[Size];
                for (Int32 i = 0; i < Size; i++)
                {
                    val = Image[i];
                    color32[i] = new Color32(val, val, val, 255);
                }

                Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                compiled.SetPixels32(color32);
                compiled.Apply(false, true);
                return compiled;
            }
            return new Texture2D(_width, _height);
        }

        // Generate a height/alpha texture from the stored data
        public override Texture2D CompileHeightAlpha()
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: compiling with unloaded map " + name + " of path " + Path + ", autoload = " +
                              AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return new Texture2D(_width, _height);
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                Color32[] color32 = new Color32[Width * Height];
                for (Int32 i = 0; i < Width * Height; i++)
                {
                    val = Image[i * 2];
                    color32[i] = new Color32(val, val, val, Image[i * 2 + 1]);
                }

                Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                compiled.SetPixels32(color32);
                compiled.Apply(false, true);
                return compiled;
            }
            return new Texture2D(_width, _height);
        }

        // Generate an RGB texture from the stored data
        public override Texture2D CompileRGB()
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: compiling with unloaded map " + name + " of path " + Path + ", autoload = " +
                              AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return new Texture2D(_width, _height);
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                Color32[] color32 = new Color32[Width * Height];
                for (Int32 i = 0; i < Width * Height; i++)
                {
                    color32[i] = new Color32(Image[i * 3], Image[i * 3 + 1], Image[i * 3 + 2], 255);
                }

                Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                compiled.SetPixels32(color32);
                compiled.Apply(false, true);
                return compiled;
            }
            return new Texture2D(_width, _height);
        }

        // Generate an RGBA texture from the stored data
        public override Texture2D CompileRGBA()
        {
            if (!IsLoaded)
            {
                if (OnDemandStorage.OnDemandLogOnMissing)
                {
                    Debug.Log("[OD] ERROR: compiling with unloaded map " + name + " of path " + Path + ", autoload = " +
                              AutoLoad);
                }

                if (AutoLoad)
                {
                    Load();
                }
                else
                {
                    return new Texture2D(_width, _height);
                }
            }

            if (OnDemandStorage.UseManualMemoryManagement)
            {
                Color32[] color32 = new Color32[Width * Height];
                for (Int32 i = 0; i < Width * Height; i++)
                {
                    color32[i] = new Color32(Image[i * 3], Image[i * 3 + 1], Image[i * 3 + 2], Image[i * 3 + 3]);
                }

                Texture2D compiled = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                compiled.SetPixels32(color32);
                compiled.Apply(false, true);
                return compiled;
            }
            return new Texture2D(_width, _height);
        }
    }

    // Parser for a MapSO
    [RequireConfigType(ConfigType.Value)]
    public class MapSOParserLarge<T> : BaseLoader, IParsable, ITypeParser<T> where T : MapSO
    {
        /// <summary>
        /// The value that is being parsed
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Parse the Value from a string
        /// </summary>
        public void SetFromString(String s)
        {
            // Should we use OnDemand?
            Boolean useOnDemand = OnDemandStorage.UseOnDemand;

            if (s.StartsWith("BUILTIN/"))
            {
                s = s.Substring(8);
                Value = Utility.FindMapSO<T>(s);
            }
            else
            {
                // are we on-demand? Don't load now.
                if (useOnDemand && typeof(T) == typeof(MapSO))
                {
                    if (!Utility.TextureExists(s))
                    {
                        return;
                    }

                    
                    MapSODemandLarge map = ScriptableObject.CreateInstance<MapSODemandLarge>();
                    map.Path = s;
                    map.AutoLoad = OnDemandStorage.OnDemandLoadOnMissing;
                    OnDemandStorage.AddMap(generatedBody.name, map);
                    Value = map as T;
                }
                else // Load the texture
                {
                    MapSODemandLarge map = ScriptableObject.CreateInstance<MapSODemandLarge>();
                    map.Path = s;
                    map.AutoLoad = false;
                    OnDemandStorage.AddMap(generatedBody.name, map);
                    Value = map as T;
                }
            }

            if (Value != null)
            {
                Value.name = s;
            }
        }

        /// <summary>
        /// Convert the value to a parsable String
        /// </summary>
        public String ValueToString()
        {
            if (Value == null)
            {
                return null;
            }

            if (GameDatabase.Instance.ExistsTexture(Value.name) || OnDemandStorage.TextureExists(Value.name))
            {
                return Value.name;
            }

            return "BUILTIN/" + Value.name;
        }

        /// <summary>
        /// Create a new MapSOParser_HeightAlpha
        /// </summary>
        public MapSOParserLarge()
        {

        }

        /// <summary>
        /// Create a new MapSOParser_HeightAlpha from an already existing Texture
        /// </summary>
        public MapSOParserLarge(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Convert Parser to Value
        /// </summary>
        public static implicit operator T(MapSOParserLarge<T> parser)
        {
            return parser.Value;
        }

        /// <summary>
        /// Convert Value to Parser
        /// </summary>
        public static implicit operator MapSOParserLarge<T>(T value)
        {
            return new MapSOParserLarge<T>(value);
        }
    }
}

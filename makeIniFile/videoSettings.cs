using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IQPowerContentManager
{
    public class VideoSettings
    {
        // [VIDEO]
        public int Width { get; set; } = 1920;
        public int Height { get; set; } = 1080;
        public int Refresh { get; set; } = 239;
        public int Fullscreen { get; set; } = 1;
        public int Vsync { get; set; } = 0;
        public int AaSamples { get; set; } = 4;
        public int Anisotropic { get; set; } = 16;
        public int ShadowMapSize { get; set; } = 4096;
        public int FpsCapMs { get; set; } = 5;
        public int Index { get; set; } = 0;
        public int AaQuality { get; set; } = 0;
        public int DisableLegacyHdr { get; set; } = 1;

        // [REFRESH]
        public int RefreshValue { get; set; } = 239;

        // [CAMERA]
        public string CameraMode { get; set; } = "DEFAULT";

        // [ASSETTOCORSA]
        public int HideArms { get; set; } = 0;
        public int HideSteer { get; set; } = 0;
        public int LockSteer { get; set; } = 0;
        public int WorldDetail { get; set; } = 5;

        // [EFFECTS]
        public int MotionBlur { get; set; } = 0;
        public int RenderSmokeInMirror { get; set; } = 1;
        public int Smoke { get; set; } = 5;
        public int Fxaa { get; set; } = 0;

        // [POST_PROCESS]
        public int PostProcessEnabled { get; set; } = 1;
        public int PostProcessQuality { get; set; } = 5;
        public string PostProcessFilter { get; set; } = "ExilitayRealism 1.07";
        public int Glare { get; set; } = 5;
        public int Dof { get; set; } = 5;
        public int RaysOfGod { get; set; } = 1;
        public int HeatShimmer { get; set; } = 1;

        // [MIRROR]
        public int MirrorHq { get; set; } = 1;
        public int MirrorSize { get; set; } = 1024;

        // [SATURATION]
        public int SaturationLevel { get; set; } = 100;

        // [CUBEMAP]
        public int CubemapFacesPerFrame { get; set; } = 6;
        public int CubemapFarplane { get; set; } = 2400;
        public int CubemapSize { get; set; } = 2048;

        /// <summary>
        /// Ustawia rozdzielczość ekranu
        /// </summary>
        /// <param name="width">Szerokość w pikselach</param>
        /// <param name="height">Wysokość w pikselach</param>
        /// <param name="refresh">Częstotliwość odświeżania w Hz</param>
        /// <param name="index">Indeks rozdzielczości (opcjonalny, domyślnie 0)</param>
        public void SetResolution(int width, int height, int refresh, int index = 0)
        {
            Width = width;
            Height = height;
            Refresh = refresh;
            RefreshValue = refresh; // Również w sekcji [REFRESH]
            Index = index;
        }

        /// <summary>
        /// Ustawia tryb wyświetlania
        /// </summary>
        /// <param name="mode">Tryb: DEFAULT (single), TRIPLE, OCULUS, OPENVR</param>
        public void SetCameraMode(string mode)
        {
            var validModes = new[] { "DEFAULT", "TRIPLE", "OCULUS", "OPENVR" };
            var upperMode = mode.ToUpper();

            if (Array.IndexOf(validModes, upperMode) >= 0)
            {
                CameraMode = upperMode;
            }
            else
            {
                throw new ArgumentException($"Nieprawidłowy tryb: {mode}. Dozwolone: DEFAULT, TRIPLE, OCULUS, OPENVR");
            }
        }

        /// <summary>
        /// Zwraca czytelną nazwę trybu wyświetlania
        /// </summary>
        public string GetCameraModeDisplayName()
        {
            return CameraMode switch
            {
                "DEFAULT" => "Single screen",
                "TRIPLE" => "Triple screen",
                "OCULUS" => "Oculus Rift",
                "OPENVR" => "OpenVR (SteamVR)",
                _ => CameraMode
            };
        }

        /// <summary>
        /// Zwraca listę dostępnych trybów wyświetlania
        /// </summary>
        public static string[] GetAvailableCameraModes()
        {
            return new[] { "DEFAULT", "TRIPLE", "OCULUS", "OPENVR" };
        }

        /// <summary>
        /// Zwraca czytelną nazwę trybu wyświetlania
        /// </summary>
        public static string GetCameraModeDisplayName(string mode)
        {
            return mode.ToUpper() switch
            {
                "DEFAULT" => "Single screen",
                "TRIPLE" => "Triple screen",
                "OCULUS" => "Oculus Rift",
                "OPENVR" => "OpenVR (SteamVR)",
                _ => mode
            };
        }

        /// <summary>
        /// Zapisuje ustawienia grafiki do pliku INI
        /// </summary>
        /// <param name="filePath">Ścieżka do pliku video.ini</param>
        public void SaveToFile(string filePath)
        {
            var sb = new StringBuilder();

            // [ASSETTOCORSA]
            sb.AppendLine("[ASSETTOCORSA]");
            sb.AppendLine($"HIDE_ARMS={HideArms}");
            sb.AppendLine($"HIDE_STEER={HideSteer}");
            sb.AppendLine($"LOCK_STEER={LockSteer}");
            sb.AppendLine($"WORLD_DETAIL={WorldDetail}");
            sb.AppendLine();

            // [CAMERA]
            sb.AppendLine("[CAMERA]");
            sb.AppendLine($"MODE={CameraMode}");
            sb.AppendLine();

            // [CUBEMAP]
            sb.AppendLine("[CUBEMAP]");
            sb.AppendLine($"FACES_PER_FRAME={CubemapFacesPerFrame}");
            sb.AppendLine($"FARPLANE={CubemapFarplane}");
            sb.AppendLine($"SIZE={CubemapSize}");
            sb.AppendLine();

            // [EFFECTS]
            sb.AppendLine("[EFFECTS]");
            sb.AppendLine($"FXAA={Fxaa}");
            sb.AppendLine($"MOTION_BLUR={MotionBlur}");
            sb.AppendLine($"RENDER_SMOKE_IN_MIRROR={RenderSmokeInMirror}");
            sb.AppendLine($"SMOKE={Smoke}");
            sb.AppendLine();

            // [MIRROR]
            sb.AppendLine("[MIRROR]");
            sb.AppendLine($"HQ={MirrorHq}");
            sb.AppendLine($"SIZE={MirrorSize}");
            sb.AppendLine();

            // [POST_PROCESS]
            sb.AppendLine("[POST_PROCESS]");
            sb.AppendLine($"DOF={Dof}");
            sb.AppendLine($"ENABLED={PostProcessEnabled}");
            sb.AppendLine($"FILTER={PostProcessFilter}");
            sb.AppendLine($"FXAA={Fxaa}");
            sb.AppendLine($"GLARE={Glare}");
            sb.AppendLine($"HEAT_SHIMMER={HeatShimmer}");
            sb.AppendLine($"QUALITY={PostProcessQuality}");
            sb.AppendLine($"RAYS_OF_GOD={RaysOfGod}");
            sb.AppendLine();

            // [REFRESH]
            sb.AppendLine("[REFRESH]");
            sb.AppendLine($"VALUE={RefreshValue}");
            sb.AppendLine();

            // [SATURATION]
            sb.AppendLine("[SATURATION]");
            sb.AppendLine($"LEVEL={SaturationLevel}");
            sb.AppendLine();

            // [VIDEO]
            sb.AppendLine("[VIDEO]");
            sb.AppendLine($"AAQUALITY={AaQuality}");
            sb.AppendLine($"AASAMPLES={AaSamples}");
            sb.AppendLine($"ANISOTROPIC={Anisotropic}");
            sb.AppendLine($"DISABLE_LEGACY_HDR={DisableLegacyHdr}");
            sb.AppendLine($"FPS_CAP_MS={FpsCapMs}");
            sb.AppendLine($"FULLSCREEN={Fullscreen}");
            sb.AppendLine($"HEIGHT={Height}");
            sb.AppendLine($"INDEX={Index}");
            sb.AppendLine($"REFRESH={Refresh}");
            sb.AppendLine($"SHADOW_MAP_SIZE={ShadowMapSize}");
            sb.AppendLine($"VSYNC={Vsync}");
            sb.AppendLine($"WIDTH={Width}");

            // Zapis do pliku (używamy Encoding.Default dla zgodności z AC)
            File.WriteAllText(filePath, sb.ToString(), Encoding.Default);
        }

        /// <summary>
        /// Wczytuje ustawienia grafiki z pliku INI
        /// </summary>
        /// <param name="filePath">Ścieżka do pliku video.ini</param>
        /// <returns>True jeśli wczytano pomyślnie, false w przeciwnym razie</returns>
        public bool LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.Default);
                var iniData = new Dictionary<string, Dictionary<string, string>>();
                string currentSection = null;

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();

                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(";") || trimmed.StartsWith("#"))
                        continue;

                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    {
                        currentSection = trimmed.Substring(1, trimmed.Length - 2).Trim();
                        if (!iniData.ContainsKey(currentSection))
                        {
                            iniData[currentSection] = new Dictionary<string, string>();
                        }
                        continue;
                    }

                    if (currentSection != null && trimmed.Contains("="))
                    {
                        var parts = trimmed.Split(new[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim();
                            var value = parts[1].Trim();
                            iniData[currentSection][key] = value;
                        }
                    }
                }

                // Wczytaj [VIDEO]
                if (iniData.ContainsKey("VIDEO"))
                {
                    var video = iniData["VIDEO"];
                    if (video.ContainsKey("WIDTH") && int.TryParse(video["WIDTH"], out int width))
                        Width = width;
                    if (video.ContainsKey("HEIGHT") && int.TryParse(video["HEIGHT"], out int height))
                        Height = height;
                    if (video.ContainsKey("REFRESH") && int.TryParse(video["REFRESH"], out int refresh))
                        Refresh = refresh;
                    if (video.ContainsKey("FULLSCREEN") && int.TryParse(video["FULLSCREEN"], out int fullscreen))
                        Fullscreen = fullscreen;
                    if (video.ContainsKey("VSYNC") && int.TryParse(video["VSYNC"], out int vsync))
                        Vsync = vsync;
                    if (video.ContainsKey("AASAMPLES") && int.TryParse(video["AASAMPLES"], out int aaSamples))
                        AaSamples = aaSamples;
                    if (video.ContainsKey("ANISOTROPIC") && int.TryParse(video["ANISOTROPIC"], out int anisotropic))
                        Anisotropic = anisotropic;
                    if (video.ContainsKey("SHADOW_MAP_SIZE") && int.TryParse(video["SHADOW_MAP_SIZE"], out int shadowMapSize))
                        ShadowMapSize = shadowMapSize;
                    if (video.ContainsKey("FPS_CAP_MS") && int.TryParse(video["FPS_CAP_MS"], out int fpsCapMs))
                        FpsCapMs = fpsCapMs;
                    if (video.ContainsKey("INDEX") && int.TryParse(video["INDEX"], out int index))
                        Index = index;
                    if (video.ContainsKey("AAQUALITY") && int.TryParse(video["AAQUALITY"], out int aaQuality))
                        AaQuality = aaQuality;
                    if (video.ContainsKey("DISABLE_LEGACY_HDR") && int.TryParse(video["DISABLE_LEGACY_HDR"], out int disableLegacyHdr))
                        DisableLegacyHdr = disableLegacyHdr;
                }

                // Wczytaj [REFRESH]
                if (iniData.ContainsKey("REFRESH"))
                {
                    var refresh = iniData["REFRESH"];
                    if (refresh.ContainsKey("VALUE") && int.TryParse(refresh["VALUE"], out int refreshValue))
                        RefreshValue = refreshValue;
                }

                // Wczytaj [CAMERA]
                if (iniData.ContainsKey("CAMERA"))
                {
                    var camera = iniData["CAMERA"];
                    if (camera.ContainsKey("MODE"))
                        CameraMode = camera["MODE"];
                }

                // Wczytaj [ASSETTOCORSA]
                if (iniData.ContainsKey("ASSETTOCORSA"))
                {
                    var ac = iniData["ASSETTOCORSA"];
                    if (ac.ContainsKey("HIDE_ARMS") && int.TryParse(ac["HIDE_ARMS"], out int hideArms))
                        HideArms = hideArms;
                    if (ac.ContainsKey("HIDE_STEER") && int.TryParse(ac["HIDE_STEER"], out int hideSteer))
                        HideSteer = hideSteer;
                    if (ac.ContainsKey("LOCK_STEER") && int.TryParse(ac["LOCK_STEER"], out int lockSteer))
                        LockSteer = lockSteer;
                    if (ac.ContainsKey("WORLD_DETAIL") && int.TryParse(ac["WORLD_DETAIL"], out int worldDetail))
                        WorldDetail = worldDetail;
                }

                // Wczytaj [EFFECTS]
                if (iniData.ContainsKey("EFFECTS"))
                {
                    var effects = iniData["EFFECTS"];
                    if (effects.ContainsKey("MOTION_BLUR") && int.TryParse(effects["MOTION_BLUR"], out int motionBlur))
                        MotionBlur = motionBlur;
                    if (effects.ContainsKey("RENDER_SMOKE_IN_MIRROR") && int.TryParse(effects["RENDER_SMOKE_IN_MIRROR"], out int renderSmokeInMirror))
                        RenderSmokeInMirror = renderSmokeInMirror;
                    if (effects.ContainsKey("SMOKE") && int.TryParse(effects["SMOKE"], out int smoke))
                        Smoke = smoke;
                    if (effects.ContainsKey("FXAA") && int.TryParse(effects["FXAA"], out int fxaa))
                        Fxaa = fxaa;
                }

                // Wczytaj [POST_PROCESS]
                if (iniData.ContainsKey("POST_PROCESS"))
                {
                    var pp = iniData["POST_PROCESS"];
                    if (pp.ContainsKey("ENABLED") && int.TryParse(pp["ENABLED"], out int enabled))
                        PostProcessEnabled = enabled;
                    if (pp.ContainsKey("QUALITY") && int.TryParse(pp["QUALITY"], out int quality))
                        PostProcessQuality = quality;
                    if (pp.ContainsKey("FILTER"))
                        PostProcessFilter = pp["FILTER"];
                    if (pp.ContainsKey("GLARE") && int.TryParse(pp["GLARE"], out int glare))
                        Glare = glare;
                    if (pp.ContainsKey("DOF") && int.TryParse(pp["DOF"], out int dof))
                        Dof = dof;
                    if (pp.ContainsKey("RAYS_OF_GOD") && int.TryParse(pp["RAYS_OF_GOD"], out int raysOfGod))
                        RaysOfGod = raysOfGod;
                    if (pp.ContainsKey("HEAT_SHIMMER") && int.TryParse(pp["HEAT_SHIMMER"], out int heatShimmer))
                        HeatShimmer = heatShimmer;
                    if (pp.ContainsKey("FXAA") && int.TryParse(pp["FXAA"], out int fxaa))
                        Fxaa = fxaa;
                }

                // Wczytaj [MIRROR]
                if (iniData.ContainsKey("MIRROR"))
                {
                    var mirror = iniData["MIRROR"];
                    if (mirror.ContainsKey("HQ") && int.TryParse(mirror["HQ"], out int hq))
                        MirrorHq = hq;
                    if (mirror.ContainsKey("SIZE") && int.TryParse(mirror["SIZE"], out int size))
                        MirrorSize = size;
                }

                // Wczytaj [SATURATION]
                if (iniData.ContainsKey("SATURATION"))
                {
                    var saturation = iniData["SATURATION"];
                    if (saturation.ContainsKey("LEVEL") && int.TryParse(saturation["LEVEL"], out int level))
                        SaturationLevel = level;
                }

                // Wczytaj [CUBEMAP]
                if (iniData.ContainsKey("CUBEMAP"))
                {
                    var cubemap = iniData["CUBEMAP"];
                    if (cubemap.ContainsKey("FACES_PER_FRAME") && int.TryParse(cubemap["FACES_PER_FRAME"], out int facesPerFrame))
                        CubemapFacesPerFrame = facesPerFrame;
                    if (cubemap.ContainsKey("FARPLANE") && int.TryParse(cubemap["FARPLANE"], out int farplane))
                        CubemapFarplane = farplane;
                    if (cubemap.ContainsKey("SIZE") && int.TryParse(cubemap["SIZE"], out int size))
                        CubemapSize = size;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas wczytywania video.ini: {ex.Message}");
                return false;
            }
        }
    }
}


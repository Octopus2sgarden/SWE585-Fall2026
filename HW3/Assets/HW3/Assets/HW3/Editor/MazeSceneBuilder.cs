using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Menu: Tools > SWE585 > Build Maze Escape Scene
/// Builds the whole playable scene (maze, player, camera, key, door, trap, guard,
/// game manager + music) into the currently open scene.
///
/// Layout legend:
///   #  wall          .  floor
///   S  player start  K  key   (rooms are on odd row/col)
///   D  exit door     T  spike trap
///   A / B  guard patrol points (guard starts at A)
/// </summary>
public static class MazeSceneBuilder
{
    const float Cell = 4f;          // room size
    const float WallThickness = 0.4f;
    const float WallHeight = 3f;
    const string RootName = "MazeGame";
    const string MatFolder = "Assets/HW3Generated";

    // Thin-wall maze: odd rows/cols are 4m rooms, even rows/cols are thin wall lines.
    // Row 0 is the far (north) side.
    // Path 1 (left, winding) : Start -> DOOR (exit, top-left)
    // Path 2 (middle)        : Start -> guard corridor (A..B) -> KEY (top-right)
    // Path 3 (right)         : Start -> spike trap (T) -> KEY
    static readonly string[] Layout =
    {
        "#D#################",
        "#.....#.#.....#..K#",
        "#.#####.#####.#.#.#",
        "#.....#.........#.#",
        "###.#.#####.#.###.#",
        "#...#...#...#.#...#",
        "#.#########.#####.#",
        "#...#A.........B#T#",
        "#.#######.#######.#",
        "#.....#.#.....#...#",
        "#.#.###.#.#######.#",
        "#.#...#.#.#.#...#.#",
        "###.###.#.#.#.###.#",
        "#........S........#",
        "###################",
    };

    [MenuItem("Tools/SWE585/Build Maze Escape Scene")]
    public static void Build()
    {
        var old = GameObject.Find(RootName);
        if (old != null) Undo.DestroyObjectImmediate(old);

        // Turn off the hand-made ground / outer walls from the first version of the scene
        // (they are only disabled, not deleted - re-enable them in the Hierarchy if needed).
        foreach (var go in EditorSceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (go.name == "Ground" || go.name == "Maze")
            {
                Undo.RecordObject(go, "Disable old maze");
                go.SetActive(false);
            }
        }

        var root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Build Maze Escape Scene");

        // ---------- Materials ----------
        var mFloor  = Existing("Assets/Materials/GroundMat.mat") ?? Mat("Floor",  new Color(0.36f, 0.40f, 0.45f));
        var mWall   = Existing("Assets/Materials/WallMat.mat") ?? Mat("Wall",   new Color(0.16f, 0.18f, 0.26f));
        var mPlayer = Mat("Player", new Color(0.20f, 0.55f, 1.00f));
        var mVisor  = Mat("Visor",  new Color(0.95f, 0.95f, 0.95f), 0.6f);
        var mKey    = Mat("Key",    new Color(1.00f, 0.78f, 0.10f), 1.2f);
        var mDoor   = Mat("Door",   new Color(0.10f, 0.85f, 0.40f), 0.5f);
        var mGuard  = Mat("Guard",  new Color(0.80f, 0.12f, 0.12f));
        var mGuardEye = Mat("GuardEye", new Color(1f, 0.9f, 0.2f), 1.5f);
        var mSpike  = Mat("Spike",  new Color(0.75f, 0.75f, 0.80f));
        var mPlate  = Mat("TrapPlate", new Color(0.25f, 0.25f, 0.25f));

        int rows = Layout.Length;
        int cols = Layout[0].Length;

        // column widths / row depths: even index = thin wall line, odd = room
        var colX = new float[cols];
        var colW = new float[cols];
        float acc = 0f;
        for (int c = 0; c < cols; c++) { colW[c] = c % 2 == 0 ? WallThickness : Cell; colX[c] = acc + colW[c] / 2f; acc += colW[c]; }
        float totalW = acc;
        var rowZ = new float[rows];
        var rowH = new float[rows];
        acc = 0f;
        for (int r = rows - 1; r >= 0; r--) { rowH[r] = r % 2 == 0 ? WallThickness : Cell; rowZ[r] = acc + rowH[r] / 2f; acc += rowH[r]; }
        float totalD = acc;

        var maze = Group("Maze", root.transform);
        var walls = Group("Walls", maze);

        // ---------- Floor ----------
        Prim(PrimitiveType.Cube, "Floor", maze,
            new Vector3(totalW / 2f, -0.5f, totalD / 2f),
            new Vector3(totalW, 1f, totalD), mFloor);

        Vector3 start = Vector3.zero, keyPos = Vector3.zero, doorPos = Vector3.zero, doorSize = Vector3.one, ga = Vector3.zero, gb = Vector3.zero;
        bool hasDoor = false, hasA = false, hasB = false;
        var traps = new List<Vector3>();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                char ch = Layout[r][c];
                Vector3 p = new Vector3(colX[c], 0f, rowZ[r]);
                switch (ch)
                {
                    case '#':
                        Prim(PrimitiveType.Cube, $"Wall_{r}_{c}", walls,
                            p + Vector3.up * WallHeight / 2f,
                            new Vector3(colW[c], WallHeight, rowH[r]), mWall);
                        break;
                    case 'S': start = p; break;
                    case 'K': keyPos = p; break;
                    case 'D': doorPos = p; hasDoor = true; doorSize = new Vector3(colW[c], WallHeight, rowH[r]); break;
                    case 'T': traps.Add(p); break;
                    case 'A': ga = p; hasA = true; break;
                    case 'B': gb = p; hasB = true; break;
                }
            }
        }

        // ---------- Player ----------
        var player = Prim(PrimitiveType.Capsule, "Player", root.transform, start + Vector3.up * 1.05f, Vector3.one, mPlayer);
        player.tag = "Player";
        player.layer = 2; // Ignore Raycast -> camera / guard / ground rays skip the player
        var visor = Prim(PrimitiveType.Cube, "Visor", player.transform, new Vector3(0f, 0.45f, 0.38f), new Vector3(0.6f, 0.18f, 0.3f), mVisor);
        Object.DestroyImmediate(visor.GetComponent<Collider>());
        visor.layer = 2;
        player.AddComponent<Rigidbody>();
        var pc = player.AddComponent<PlayerController>();

        // ---------- Key ----------
        var key = Prim(PrimitiveType.Cube, "Key", root.transform, keyPos + Vector3.up * 0.3f, Vector3.one * 0.6f, mKey);
        var krb = key.AddComponent<Rigidbody>();
        krb.mass = 1f;
        key.AddComponent<KeyObject>();
        AddPointLight(key.transform, Vector3.up * 2f, new Color(1f, 0.8f, 0.2f), 5f, 2f);

        // ---------- Door ----------
        if (hasDoor)
        {
            var door = Prim(PrimitiveType.Cube, "Door", maze, doorPos + Vector3.up * WallHeight / 2f, doorSize, mDoor);
            door.AddComponent<Door>();
            AddPointLight(maze, doorPos + Vector3.up * 2.5f, new Color(0.2f, 1f, 0.4f), 8f, 2.5f);
        }
        else Debug.LogWarning("No 'D' (door) in layout.");

        // ---------- Spike traps ----------
        foreach (var tp in traps)
        {
            var trapGo = new GameObject("SpikeTrap");
            trapGo.transform.SetParent(maze, false);
            trapGo.transform.localPosition = tp;

            var plate = Prim(PrimitiveType.Cube, "WarningPlate", trapGo.transform,
                new Vector3(0f, 0.01f, 0f), new Vector3(Cell * 0.95f, 0.02f, Cell * 0.95f), mPlate);
            Object.DestroyImmediate(plate.GetComponent<Collider>());

            var spikes = new GameObject("Spikes");
            spikes.transform.SetParent(trapGo.transform, false);
            const int n = 6;
            float spacing = Cell / n;
            for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                var sp = Prim(PrimitiveType.Cube, "Spike", spikes.transform,
                    new Vector3(-Cell / 2f + spacing * (i + 0.5f), 0.5f, -Cell / 2f + spacing * (j + 0.5f)),
                    new Vector3(0.16f, 1f, 0.16f), mSpike);
                sp.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
                Object.DestroyImmediate(sp.GetComponent<Collider>()); // visual only, key can pass
            }

            var zone = new GameObject("KillZone");
            zone.transform.SetParent(trapGo.transform, false);
            zone.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            var bc = zone.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(Cell * 0.9f, 1.3f, Cell * 0.9f);

            var st = trapGo.AddComponent<SpikeTrap>();
            st.spikes = spikes.transform;
            st.warningPlate = plate.GetComponent<Renderer>();
            var kz = zone.AddComponent<TrapKillZone>();
            kz.trap = st;
        }

        // ---------- Guard ----------
        if (hasA && hasB)
        {
            var patrol = Group("GuardPatrol", maze);
            var pa = new GameObject("PointA").transform;
            pa.SetParent(patrol, false);
            pa.localPosition = ga + Vector3.up;
            var pb = new GameObject("PointB").transform;
            pb.SetParent(patrol, false);
            pb.localPosition = gb + Vector3.up;

            var guard = Prim(PrimitiveType.Capsule, "Guard", root.transform, ga + Vector3.up, Vector3.one, mGuard);
            var eye = Prim(PrimitiveType.Cube, "Eye", guard.transform, new Vector3(0f, 0.5f, 0.4f), new Vector3(0.7f, 0.15f, 0.25f), mGuardEye);
            Object.DestroyImmediate(eye.GetComponent<Collider>());
            var grb = guard.AddComponent<Rigidbody>();
            grb.isKinematic = true;
            var g = guard.AddComponent<Guard>();
            g.pointA = pa;
            g.pointB = pb;
            g.player = player.transform;

            var lgo = new GameObject("VisionLight");
            lgo.transform.SetParent(guard.transform, false);
            lgo.transform.localPosition = new Vector3(0f, 0.6f, 0.3f);
            lgo.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
            var l = lgo.AddComponent<Light>();
            l.type = LightType.Spot;
            l.color = new Color(1f, 0.3f, 0.2f);
            l.spotAngle = g.viewAngle;
            l.range = g.viewDistance + 2f;
            l.intensity = 4f;
        }
        else Debug.LogWarning("Layout needs both 'A' and 'B' for the guard.");

        // ---------- Camera ----------
        Camera cam = Camera.main;
        if (cam == null)
        {
            var cg = new GameObject("Main Camera");
            cg.tag = "MainCamera";
            cam = cg.AddComponent<Camera>();
            cg.AddComponent<AudioListener>();
        }
        var tpc = cam.GetComponent<ThirdPersonCamera>();
        if (tpc == null) tpc = cam.gameObject.AddComponent<ThirdPersonCamera>();
        tpc.target = player.transform;
        cam.transform.position = start + new Vector3(0f, 4f, -7f);
        cam.transform.LookAt(start);
        pc.cameraTransform = cam.transform;

        // ---------- Light ----------
        if (GameObject.Find("Directional Light") == null)
        {
            var lg = new GameObject("Directional Light");
            var dl = lg.AddComponent<Light>();
            dl.type = LightType.Directional;
            dl.intensity = 1f;
            dl.shadows = LightShadows.Soft;
            lg.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // ---------- Game manager + music ----------
        var gmGo = new GameObject("GameManager");
        gmGo.transform.SetParent(root.transform, false);
        var src = gmGo.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        src.volume = 0.35f;
        var gm = gmGo.AddComponent<GameManager>();
        gm.player = pc;
        gm.music = src;

        AssetDatabase.SaveAssets();
        Selection.activeGameObject = root;
        EditorSceneManager.MarkSceneDirty(root.scene);
        AddSceneToBuildSettings();
        Debug.Log("Maze Escape scene built. Save the scene (Ctrl+S) and press Play.");
    }

    // ------------------------------------------------------------------

    static Transform Group(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static void AddPointLight(Transform parent, Vector3 localPos, Color color, float range, float intensity)
    {
        var go = new GameObject("PointLight");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.range = range;
        l.intensity = intensity;
    }

    static Shader LitShader()
    {
        var rp = GraphicsSettings.currentRenderPipeline;
        if (rp != null)
        {
            string n = rp.GetType().Name;
            if (n.Contains("Universal"))
            {
                var s = Shader.Find("Universal Render Pipeline/Lit");
                if (s != null) return s;
            }
            if (n.Contains("HD"))
            {
                var s = Shader.Find("HDRP/Lit");
                if (s != null) return s;
            }
        }
        return Shader.Find("Standard");
    }

    // Use the project's own material if it exists (plain C# null -> safe with ??)
    static Material Existing(string path)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        return m != null ? m : null;
    }

    static Material Mat(string name, Color color, float emission = 0f)
    {
        if (!AssetDatabase.IsValidFolder(MatFolder))
            AssetDatabase.CreateFolder("Assets", "HW3Generated");

        string path = $"{MatFolder}/{name}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(LitShader());
            AssetDatabase.CreateAsset(m, path);
        }
        m.color = color;
        if (emission > 0f)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", color * emission);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        EditorUtility.SetDirty(m);
        return m;
    }

    static void AddSceneToBuildSettings()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(scene.path))
        {
            Debug.LogWarning("Scene is not saved yet. Save it (Ctrl+S), then run the builder again so it is added to Build Settings.");
            return;
        }
        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!list.Exists(s => s.path == scene.path))
        {
            list.Insert(0, new EditorBuildSettingsScene(scene.path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}

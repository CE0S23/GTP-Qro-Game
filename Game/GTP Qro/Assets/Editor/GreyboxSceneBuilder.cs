using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace GTP.EditorTools
{
    /// <summary>
    /// Genera las 7 escenas greybox del GDD (§8.1) con primitivas, materiales URP y todos los
    /// componentes ya conectados (eventos en el Inspector). Menú: GTP > Construir escenas greybox.
    /// ADVERTENCIA: sobrescribe Assets/_Scenes/Escena_*.unity. Si se editan escenas a mano,
    /// no volver a ejecutar o pasar esos cambios a este script.
    /// </summary>
    public static partial class GreyboxSceneBuilder
    {
        private const string SceneDir = "Assets/_Scenes";
        private const string MatDir = "Assets/Materials/Greybox";
        private static readonly Vector3 CamOffset = new Vector3(0f, 14f, -10f);
        private static Quaternion CamRotation => Quaternion.LookRotation(-CamOffset, Vector3.up);

        private static Dictionary<string, Material> mats;
        private static Font font;
        private static int counter;

        [MenuItem("GTP/Construir escenas greybox")]
        public static void BuildAllMenu()
        {
            if (!EditorUtility.DisplayDialog("Construir escenas greybox",
                    "Se regenerarán las escenas Escena_* en Assets/_Scenes. Los cambios manuales en esas escenas se perderán.",
                    "Construir", "Cancelar"))
                return;
            BuildAll();
        }

        /// <summary>Punto de entrada para línea de comandos (-executeMethod).</summary>
        public static void BuildAllBatch()
        {
            try
            {
                BuildAll();
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }

        public static void BuildAll()
        {
            EnsureFolder(SceneDir);
            EnsureFolder(MatDir);
            EnsureFolder("Assets/Resources");
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            CreateMaterials();
            EnsureConfig();
            PrepareHito03Assets();

            BuildMenu();
            BuildCuarto();
            BuildBarrio();
            BuildEncrucijada();
            BuildRutaBien();
            BuildRutaMal();
            BuildSpot();

            var scenes = new List<EditorBuildSettingsScene>();
            foreach (string name in SceneNames.BuildOrder)
                scenes.Add(new EditorBuildSettingsScene(SceneDir + "/" + name + ".unity", true));
            EditorBuildSettings.scenes = scenes.ToArray();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[GTP] Escenas greybox construidas: " + string.Join(", ", SceneNames.BuildOrder));
        }

        // =====================================================================
        // ESCENAS
        // =====================================================================

        private static void BuildMenu()
        {
            NewScene();
            Lighting(new Color(0.45f, 0.5f, 0.9f), 0.35f, new Color(0.15f, 0.15f, 0.28f), new Vector3(50f, -30f, 0f));
            Ground(0f, 0f, 40f, 30f, "Asfalto");
            for (int i = -3; i <= 3; i++)
                Box("Edificio_" + i, i * 6f, 8f, 4.5f, 4f, 2f + Mathf.Abs(i) * 0.7f, "Edificio");
            Lamp(-6f, 2f);
            Lamp(6f, 2f);
            Label3D("GTP", new Vector3(0f, 1f, 3f), new Color(1f, 0.8f, 0.3f), 0.25f);
            var cam = CreateCamera(null, new Vector3(0f, 9f, -12f));
            cam.transform.rotation = Quaternion.LookRotation(new Vector3(0f, -9f, 15f));
            new GameObject("MainMenu").AddComponent<MainMenu>();
            Save(SceneNames.Menu);
        }

        private static void BuildCuarto()
        {
            NewScene();
            Lighting(new Color(1f, 0.62f, 0.38f), 0.9f, new Color(0.38f, 0.28f, 0.26f), new Vector3(35f, -60f, 0f));
            Ground(0f, 0f, 13f, 10f, "Madera");
            Ground(7.75f, 0f, 3f, 2.5f, "Madera");
            Box("Pared_N", 0f, 4.75f, 12.5f, 0.5f, 2.5f, "Pared");
            Box("Pared_O", -6.25f, 0f, 0.5f, 10f, 2.5f, "Pared");
            Box("Pared_E1", 6.25f, 3f, 0.5f, 4f, 2.5f, "Pared");
            Box("Pared_E2", 6.25f, -3f, 0.5f, 4f, 2.5f, "Pared");
            Box("Pared_S", 0f, -4.75f, 12.5f, 0.5f, 0.6f, "Pared");
            Box("Pasillo_N", 7.75f, 1.4f, 3f, 0.3f, 2f, "Pared");
            Box("Pasillo_S", 7.75f, -1.4f, 3f, 0.3f, 0.6f, "Pared");
            Box("Pasillo_Fin", 9.4f, 0f, 0.3f, 3f, 2f, "Pared");
            Box("Cama", -4f, 2.6f, 3f, 2f, 0.7f, "Cama");
            Box("Escritorio", 3.8f, 3.4f, 2.6f, 1.2f, 0.9f, "Escritorio");
            Box("Repisa", -5.6f, -2.5f, 0.8f, 2.5f, 1.6f, "Escritorio");

            GameObject player = Player(new Vector3(-2f, 1f, -1f));
            CreateCamera(player.transform, null);

            GameObject phone = Prop("Celular", new Vector3(3.8f, 0.95f, 3.4f), new Vector3(0.35f, 0.06f, 0.6f), "Celular");
            LevelExit exit = Exit("Salida_Barrio", 8.5f, 0f, 1f, 2.4f, SceneNames.Barrio, true,
                "Primero debería revisar mi celular.");

            ObjectiveStep revisar = Objective("Objetivo_Celular", "Revisa tu celular (escritorio)", phone.transform, true);
            ObjectiveStep salir = Objective("Objetivo_Salir", "Sal por la puerta", exit.transform, false);

            var phoneTalk = phone.AddComponent<NarrativeInteractable>();
            phoneTalk.Configure("Revisar celular", new[]
            {
                L("Celular", "Batería: 12 %."),
                L("Celular", "Mensajes a tu novia: 3 sin respuesta."),
                L("Pensamiento", "Ya no queda nada. Voy al Punto."),
                L("Tutorial", "La batería baja con el tiempo; si llega a 0 % pierdes. Recoge baterías portátiles (cubos azules) en el camino.")
            }, new[] { L("Celular", "Sigue sin contestar...") }, true);
            On(phoneTalk.OnFirstFinished, exit.Unlock);
            On(phoneTalk.OnFirstFinished, salir.Activate);

            GameObject headphones = Prop("Audifonos", new Vector3(-4.6f, 0.8f, 2.6f), new Vector3(0.6f, 0.15f, 0.4f), "Grafiti");
            headphones.AddComponent<NarrativeInteractable>().Configure("Tomar audífonos", new[]
            {
                L("Pensamiento", "Sin música no salgo."),
                L("", "Te pones los audífonos. El beat suave del atardecer te acompaña.")
            }, new[] { L("Pensamiento", "Ya los traigo puestos.") }, true);

            Graffiti("Grafiti_Foto", -1f, 4.4f, "Foto: tú, tu novia y tus amigos en el Spot",
                "Hace mucho que no salimos juntos...", 2.2f, 0.12f);

            Sequence("Intro", true,
                L("", "Atardecer en Querétaro."),
                L("Pensamiento", "Otro día igual... y ella sigue sin contestar."),
                L("Tutorial", "Muévete con WASD o flechas. Shift: correr. C: agacharse. E: interactuar. Esc: pausa."),
                L("Tutorial", "Tienes 3 intentos por nivel. Lee mensajes y recoge baterías para sumar puntos."));
            _ = revisar;
            Save(SceneNames.Cuarto);
        }

        private static void BuildBarrio()
        {
            NewScene();
            Night();
            Ground(60f, 0f, 124f, 22f, "Asfalto");
            Bounds(0f, 120f, -10f, 10f);
            // Edificios del lado norte (fondo) y bardas bajas al sur (no tapan la cámara).
            Box("Edificio_N1", 5f, 8.5f, 10f, 3f, 2.5f, "Edificio");
            Box("Edificio_N2", 20f, 8.5f, 12f, 3f, 2.5f, "Edificio");
            Box("Edificio_N3", 34f, 8.5f, 8f, 3f, 2.5f, "Edificio");
            Box("Edificio_N4", 50f, 8.5f, 12f, 3f, 2.5f, "Edificio");
            Box("Edificio_N5", 104f, 8.5f, 8f, 3f, 2.5f, "Edificio");
            Box("Barda_S1", 8f, -8.5f, 10f, 3f, 1.2f, "Pared");
            Box("Barda_S2", 24f, -8.5f, 10f, 3f, 1.2f, "Pared");
            Box("Barda_S3", 46f, -8.5f, 8f, 3f, 1.2f, "Pared");
            foreach (float x in new[] { 6f, 30f, 54f, 78f, 102f }) Lamp(x, 6f);
            foreach (float x in new[] { 18f, 42f, 66f, 90f, 114f }) Lamp(x, -6f);

            GameObject player = Player(new Vector3(3f, 1f, 0f));
            CreateCamera(player.transform, null);

            // 1D · El Punto (se crea antes para que el objetivo apunte a la puerta)
            Box("El_Punto", 113f, 6f, 10f, 4f, 2.5f, "Punto");
            Label3D("EL PUNTO", new Vector3(113f, 3.2f, 4f), new Color(1f, 0.4f, 0.3f), 0.12f);
            GameObject door = Prop("Puerta_Punto", new Vector3(113f, 1.1f, 3.9f), new Vector3(2f, 2.2f, 0.2f), "Puerta");
            LevelExit exit = Exit("Salida_Encrucijada", 119f, -5f, 2f, 8f, SceneNames.Encrucijada, true,
                "Primero quiero ver si el Punto está abierto.");

            Objective("Objetivo_Punto", "Llega al Punto (sigue la flecha del mapa)", door.transform, true);
            ObjectiveStep toPlaza = Objective("Objetivo_Plaza", "Ve a la plaza a buscar a Orlando", exit.transform, false);
            Sequence("Intro", true,
                L("Pensamiento", "El Punto está al final de esta calle. El mapa del celular marca el camino."),
                L("Tutorial", "Lee los grafitis con E: suman puntos y dicen cosas que vale la pena escuchar."));

            // 1A · Orientación
            Graffiti("Grafiti_NoEsSalida", 18f, 6.9f, "NO ES UNA SALIDA", "¿Y si lo que busco no me va a ayudar?");
            Pickup("Bateria_01", 24f, -5f);

            // 1B · Asalto
            Zone("Aviso_Asalto", 28f, -1.5f, 1.5f, 17f, "Este tramo está feo... si alguien me sigue, corro (Shift).");
            Container("Contenedor_01", 40f, -1f);
            Box("Auto_01", 48f, 4f, 3.5f, 1.6f, 1.3f, "Auto");
            Box("Auto_02", 50f, -5f, 3.5f, 1.6f, 1.3f, "Auto");
            ChaserNPC mugger = Chaser("Asaltante", new Vector3(42.6f, 1f, -1f), "Asaltante", 5f, 6f, 22f,
                "El asaltante te alcanzó y te arrebató el celular un momento.", "Asaltante: ¡Dame el celular!", false, false);
            TriggerZone safe = Zone("Zona_Segura", 56f, -1.5f, 1.5f, 17f, "Lo perdí... Ahora, cuidado con la patrulla.");
            On(safe.OnPlayerEnter, mugger.GiveUp);
            Checkpoint("Checkpoint_01", 58.5f, -1.5f, 17f);

            // 1C · Sigilo con patrullas
            var section = new GameObject("Tramo_Sigilo_Barrio").AddComponent<StealthSection>();
            On(Zone("Sigilo_Inicio", 61f, -1.5f, 1.5f, 17f, "Una patrulla. Me agacho (C) y me cubro detrás de los autos.").OnPlayerEnter, section.Begin);
            On(Zone("Sigilo_Fin", 96f, -1.5f, 1.5f, 17f, "").OnPlayerEnter, section.Complete);
            Box("Auto_03", 66f, 3f, 3.5f, 1.6f, 1.3f, "Auto");
            Box("Auto_04", 68f, -5f, 3.5f, 1.6f, 1.3f, "Auto");
            Box("Auto_05", 76f, 1f, 1.6f, 3.5f, 1.3f, "Auto");
            Box("Auto_06", 84f, 4.5f, 3.5f, 1.6f, 1.3f, "Auto");
            Box("Auto_07", 84f, -4f, 3.5f, 1.6f, 1.3f, "Auto");
            Box("Auto_08", 91f, -1f, 1.6f, 3.5f, 1.3f, "Auto");
            Container("Contenedor_02", 79f, -8.6f);
            Container("Contenedor_03", 88f, 7.2f);
            Police("Policia_A", new[] { V(72f, -8f), V(72f, 6f) }, true, 2f, 1.5f);
            Police("Policia_B", new[] { V(80f, -8f), V(94f, -8f), V(94f, 6f), V(80f, 6f) }, false, 2.2f, 1f);
            Pickup("Bateria_02", 82f, -1f);
            Checkpoint("Checkpoint_02", 98f, -1.5f, 17f);

            // 1D · Punto cerrado
            Graffiti("Grafiti_TuVida", 102f, 6.9f, "TU VIDA VALE MÁS", "Últimamente se me olvida.");
            Pickup("Bateria_03", 106f, -7f);
            var doorTalk = door.AddComponent<NarrativeInteractable>();
            doorTalk.Configure("Tocar la puerta", new[]
            {
                L("", "La cortina está abajo. No hay nadie."),
                L("Pensamiento", "Está cerrado... ¿Y ahora?"),
                L("Pensamiento", "Orlando dijo que andaría por la plaza. Igual sabe de otro lugar.")
            }, new[] { L("Pensamiento", "Sigue cerrado.") }, true);
            On(doorTalk.OnFirstFinished, exit.Unlock);
            On(doorTalk.OnFirstFinished, toPlaza.Activate);
            Save(SceneNames.Barrio);
        }

        private static void BuildEncrucijada()
        {
            NewScene();
            Lighting(new Color(0.55f, 0.5f, 0.9f), 0.3f, new Color(0.24f, 0.2f, 0.36f), new Vector3(55f, -20f, 0f));
            Ground(0f, 0f, 32f, 26f, "Banqueta");
            Bounds(-15f, 15f, -12f, 12f);
            Box("Fuente", 0f, -1f, 4f, 4f, 1f, "Pared");
            Box("Banca_1", -7f, -2f, 3f, 1f, 0.5f, "Escritorio");
            Box("Banca_2", 7f, -2f, 3f, 1f, 0.5f, "Escritorio");
            Lamp(-10f, 0f);
            Lamp(10f, 0f);
            Lamp(0f, 9f);
            Label3D("← Con tu novia y tu amigo", new Vector3(-9f, 1.5f, 8f), new Color(0.6f, 1f, 0.7f), 0.08f);
            Label3D("Nuevo punto →", new Vector3(9f, 1.5f, 8f), new Color(1f, 0.5f, 0.5f), 0.08f);

            GameObject player = Player(new Vector3(0f, 1f, -9f));
            CreateCamera(player.transform, null);

            GameObject orlando = Npc("Orlando", new Vector3(0f, 1f, 4.5f), "Orlando", false);
            orlando.transform.rotation = Quaternion.LookRotation(Vector3.back);
            var branching = new GameObject("BranchingManager").AddComponent<BranchingManager>();
            var talk = orlando.AddComponent<NarrativeInteractable>();
            talk.Configure("Hablar con Orlando", new[]
            {
                L("Orlando", "¿Qué onda? ¿Buscando lo mismo?"),
                L("Protagonista", "El Punto está cerrado."),
                L("Orlando", "Sé de otro lugar. Está caliente con la poli, pero hay."),
                L("Pensamiento", "Mi novia no contesta... pero mi amigo dijo que estaría con ella en el parque."),
                L("Pensamiento", "Puedo seguir buscando... o ir con ellos.")
            }, new[] { L("Orlando", "¿Entonces qué? ¿Vienes?") }, true);
            On(talk.OnFirstFinished, branching.OpenDecision);

            Objective("Objetivo_Orlando", "Habla con Orlando", orlando.transform, true);
            Sequence("Intro", true, L("Pensamiento", "La plaza. Ahí está Orlando."));
            Graffiti("Grafiti_Necesitas", -8f, 11.4f, "¿DE VERDAD LO NECESITAS?", "Tal vez no...");
            Pickup("Bateria_04", 11f, -7f);
            Save(SceneNames.Encrucijada);
        }

        private static void BuildRutaBien()
        {
            NewScene();
            Lighting(new Color(0.65f, 0.6f, 0.9f), 0.4f, new Color(0.22f, 0.2f, 0.3f), new Vector3(50f, -40f, 0f));
            Ground(35f, 0f, 74f, 22f, "Pasto");
            Ground(35f, 0f, 74f, 4f, "Banqueta", 0.01f);
            Bounds(0f, 70f, -10f, 10f);
            Box("Barda_N1", 12f, 9.2f, 10f, 1.5f, 2.2f, "Edificio");
            Box("Barda_N2", 32f, 9.2f, 10f, 1.5f, 2.2f, "Edificio");
            Box("Barda_N3", 55f, 9.2f, 10f, 1.5f, 2.2f, "Edificio");
            foreach (Vector3 t in new[] { V(15f, -5f), V(28f, 3.5f), V(38f, 6f), V(47f, -6f), V(60f, -3f), V(8f, 5f) })
                Box("Arbol_" + (counter++), t.x, t.z, 1.4f, 1.4f, 2.2f, "Arbol");
            foreach (float x in new[] { 10f, 25f, 40f, 55f }) Lamp(x, -3f);

            GameObject player = Player(new Vector3(3f, 1f, 0f));
            CreateCamera(player.transform, null);

            GameObject novia = Npc("Novia", new Vector3(20f, 1f, 5f), "Novia", true);
            GameObject amigo = Npc("Amigo", new Vector3(42f, 1f, -4f), "Amigo", true);
            novia.GetComponent<CompanionFollower>().Configure(false, new Vector3(-1.2f, 0f, -1.2f));
            amigo.GetComponent<CompanionFollower>().Configure(false, new Vector3(1.2f, 0f, -1.4f));
            LevelExit exit = Exit("Salida_Spot", 68f, 0f, 2f, 8f, SceneNames.Spot, true, "Falta encontrar a mi amigo.");

            ObjectiveStep o1 = Objective("Objetivo_Novia", "Encuentra a tu novia en el parque", novia.transform, true);
            ObjectiveStep o2 = Objective("Objetivo_Amigo", "Encuentra a tu amigo", amigo.transform, false);
            ObjectiveStep o3 = Objective("Objetivo_Spot", "Lleguen juntos al Spot", exit.transform, false);
            _ = o1;

            var amigoTalk = amigo.AddComponent<NarrativeInteractable>();
            amigoTalk.Configure("Hablar con tu amigo", new[]
            {
                L("Amigo", "¡Ey! Pensé que ya no venías."),
                L("Amigo", "Vamos al Spot a ver el amanecer, como antes."),
                L("Pensamiento", "Hace mucho que no me sentía así.")
            }, new[] { L("Amigo", "¡Vámonos, que ya va a salir el sol!") }, false);
            On(amigoTalk.OnFirstFinished, amigo.GetComponent<CompanionFollower>().StartFollowing);
            On(amigoTalk.OnFirstFinished, exit.Unlock);
            On(amigoTalk.OnFirstFinished, o3.Activate);

            var noviaTalk = novia.AddComponent<NarrativeInteractable>();
            noviaTalk.Configure("Hablar con tu novia", new[]
            {
                L("Novia", "¡Por fin! Te estuve marcando; me quedé sin pila toda la tarde."),
                L("Novia", "Tu amigo está más adelante. Vamos juntos."),
                L("Pensamiento", "Esto sí vale la pena.")
            }, new[] { L("Novia", "Aquí estoy, vamos.") }, true);
            On(noviaTalk.OnFirstFinished, novia.GetComponent<CompanionFollower>().StartFollowing);
            OnBool(noviaTalk.OnFirstFinished, amigoTalk.SetAvailable, true);
            On(noviaTalk.OnFirstFinished, o2.Activate);

            GameObject bench = Prop("Banca", new Vector3(33f, 0.25f, -2.6f), new Vector3(3f, 0.5f, 1f), "Escritorio");
            bench.AddComponent<NarrativeInteractable>().Configure("Sentarse un momento", new[]
            {
                L("Pensamiento", "Me siento un momento. El ruido de la ciudad se oye lejos."),
                L("Pensamiento", "Toda la noche buscando algo para no sentir... y lo que necesitaba era esto."),
            }, new[] { L("Pensamiento", "Se está bien aquí.") }, true);

            Sequence("Intro", true,
                L("Pensamiento", "Mejor voy con ellos. Le marco otra vez..."),
                L("Celular", "Novia: «Estoy en el parque con tu amigo. Ven.»"));
            Graffiti("Grafiti_NoEsSalida", 12f, 8.35f, "NO ES UNA SALIDA", "Esto sí vale la pena.");
            Graffiti("Grafiti_PideAyuda", 32f, 8.35f, "PIDE AYUDA, NO ESTÁS SOLO", "Pedir ayuda no es de débiles.");
            Graffiti("Grafiti_LosTuyos", 55f, 8.35f, "LA SALIDA ES CON LOS TUYOS", "Ya entendí.");
            Pickup("Bateria_05", 26f, -6f);
            Pickup("Bateria_06", 50f, 6f);
            Save(SceneNames.RutaBien);
        }

        private static void BuildRutaMal()
        {
            NewScene();
            Lighting(new Color(0.35f, 0.4f, 0.8f), 0.22f, new Color(0.17f, 0.17f, 0.3f), new Vector3(60f, -30f, 0f));
            Ground(65f, 0f, 134f, 22f, "Asfalto");
            Bounds(0f, 130f, -10f, 10f);
            Box("Bodega", 10f, 8f, 16f, 4f, 2.5f, "Edificio");
            Box("Bodega_S", 10f, -8.5f, 16f, 3f, 1.2f, "Pared");
            foreach (float x in new[] { 6f, 20f, 34f, 48f }) Lamp(x, -6f, new Color(1f, 0.3f, 0.3f));
            foreach (float x in new[] { 90f, 104f, 118f }) Lamp(x, 6f);

            GameObject player = Player(new Vector3(3f, 1f, 0f));
            CreateCamera(player.transform, null);
            GameObject orlando = Npc("Orlando", new Vector3(1.5f, 1f, 1.5f), "Orlando", true);
            orlando.GetComponent<CompanionFollower>().Configure(true, new Vector3(-1.3f, 0f, 1.3f));

            GameObject puntoMarker = Marker("Nuevo_Punto", 18f, 0f);
            Label3D("NUEVO PUNTO", new Vector3(18f, 3f, 6f), new Color(1f, 0.4f, 0.3f), 0.1f);
            Objective("Objetivo_NuevoPunto", "Sigue a Orlando al nuevo punto", puntoMarker.transform, true);
            Sequence("Intro", true,
                L("Orlando", "Por aquí. El nuevo punto está pasando la bodega."),
                L("Pensamiento", "No me gusta este lugar..."));

            // Redada: el grupo se activa por evento al llegar al nuevo punto.
            var redada = new GameObject("Redada");
            GameObject gate = Box("Porton", 62.5f, 0f, 0.6f, 4f, 2.2f, "Puerta");
            DynamicObstacle(gate); // al forzarlo se desactiva y el NavMesh se abre en tiempo real
            ObjectiveStep toGate = Objective("Objetivo_Porton", "Llega al portón del callejón y fuerza la cerradura", gate.transform, false);
            NarrativeSequence raid = Sequence("Redada_Inicio", false,
                L("Orlando", "Aquí es. Espérame, voy a ver si..."),
                L("", "¡SIRENAS! Luces rojas y azules por todas partes."),
                L("Policía (megáfono)", "¡NADIE SE MUEVE!"),
                L("Orlando", "¡Redada! ¡Por el callejón, hay que forzar el portón!"),
                L("Tutorial", "Escóndete en contenedores (E). Evita los conos de visión y el foco del helicóptero."));
            OnBool(raid.OnFinished, redada.SetActive, true);
            On(raid.OnFinished, toGate.Activate);
            On(Zone("Llegada_Punto", 16f, 0f, 1.5f, 17f, "").OnPlayerEnter, raid.Play);

            var section = new GameObject("Tramo_Sigilo_Redada").AddComponent<StealthSection>();
            On(Zone("Sigilo_Inicio", 22f, -1.5f, 1.5f, 17f, "").OnPlayerEnter, section.Begin);
            foreach (Vector3 c in new[] { V(26f, 4f), V(30f, -5f), V(36f, 1f), V(42f, 5.5f), V(44f, -6f), V(50f, 0f), V(56f, 5f), V(57f, -5f) })
                Box("Cobertura_" + (counter++), c.x, c.z, 3f, 1.6f, 1.3f, "Auto");
            Container("Contenedor_01", 38f, -8.6f);
            Container("Contenedor_02", 47f, 7.2f);
            Container("Contenedor_03", 59f, 1.5f);
            Pickup("Bateria_07", 40f, -1f);
            GameObject p1 = Police("Policia_R1", new[] { V(28f, -8f), V(28f, 6f) }, true, 2f, 1.2f);
            GameObject p2 = Police("Policia_R2", new[] { V(38f, -3f), V(46f, -3f), V(46f, 3f), V(38f, 3f) }, false, 2f, 1f);
            GameObject p3 = Police("Policia_R3", new[] { V(53f, -8f), V(53f, 6f) }, true, 2.4f, 1f);
            GameObject light = Searchlight("Foco_Helicoptero", new[] { V(26f, -4f), V(40f, 4f), V(56f, -4f) }, 3f);
            foreach (GameObject g in new[] { p1, p2, p3, light }) g.transform.SetParent(redada.transform, true);
            redada.SetActive(false);

            // Callejón y portón
            Box("Callejon_N", 71f, 6f, 18f, 8f, 2.5f, "Edificio");
            Box("Callejon_S", 71f, -6f, 18f, 8f, 1.2f, "Pared");
            var lockGate = gate.AddComponent<LockedGate>();
            lockGate.Configure(gate);
            On(lockGate.OnOpened, section.Complete);
            Checkpoint("Checkpoint_Callejon", 66f, 0f, 3.5f);
            Graffiti("Grafiti_Dosis", 72f, 1.9f, "CADA DOSIS TE ALEJA", "¿A dónde me está llevando esto?", 2f, 0.12f);
            ObjectiveStep toHouse = Objective("Objetivo_Casa", "Escapa por el callejón hacia casa de tu novia", null, false);
            On(lockGate.OnOpened, toHouse.Activate);

            // Casa de la novia: revelación y huida
            Box("Casa_Novia", 92f, 7.5f, 9f, 4f, 2.5f, "Novia");
            Label3D("Casa de tu novia", new Vector3(92f, 3.2f, 5.4f), new Color(0.9f, 0.6f, 1f), 0.08f);
            Box("Auto_H1", 100f, -3f, 3.5f, 1.6f, 1.3f, "Auto");
            Box("Auto_H2", 108f, 4f, 3.5f, 1.6f, 1.3f, "Auto");
            Box("Auto_H3", 116f, -4f, 3.5f, 1.6f, 1.3f, "Auto");
            Pickup("Bateria_08", 102f, 6f);
            LevelExit exit = Exit("Salida_Spot", 128f, 0f, 2f, 10f, SceneNames.Spot, false, "");
            ChaserNPC cop = Chaser("Policia_Altercado", new Vector3(84f, 1f, -8f), "Policia", 5.3f, 0f, 60f,
                "El policía te alcanzó.", "", true, true);
            ObjectiveStep run = Objective("Objetivo_Huir", "¡Huye al Spot! (Shift para correr)", exit.transform, false);
            NarrativeSequence reveal = Sequence("Revelacion", false,
                L("Pensamiento", "La casa de mi novia. No contestaba... le voy a tocar."),
                L("", "[Panel de cómic] Por la ventana: ella, abrazada con otra persona."),
                L("Pensamiento", "¿Por eso no contestaba?"),
                L("Orlando", "Wey... lo siento. Yo me voy, aquí ya se puso feo."),
                L("Policía", "¡TÚ! ¡Alto ahí!"),
                L("Tutorial", "¡Corre hasta el Spot! No puedes pelear."));
            OnBool(reveal.OnFinished, orlando.SetActive, false);
            On(reveal.OnFinished, cop.Activate);
            On(reveal.OnFinished, run.Activate);
            Checkpoint("Checkpoint_Casa", 85f, -1.5f, 17f);
            On(Zone("Llegada_Casa", 86.5f, -1.5f, 1.5f, 17f, "").OnPlayerEnter, reveal.Play);
            Save(SceneNames.RutaMal);
        }

        private static void BuildSpot()
        {
            NewScene();
            Light sun = Lighting(new Color(1f, 0.6f, 0.45f), 0.6f, new Color(0.35f, 0.3f, 0.4f), new Vector3(15f, -20f, 0f));
            Ground(0f, 0f, 30f, 24f, "Pasto");
            Bounds(-14f, 14f, -11f, 11f);
            Box("Mirador", 0f, 9f, 10f, 2f, 0.8f, "Pared");
            Box("Banca_Spot", 0f, 6.8f, 4f, 1f, 0.5f, "Escritorio");
            Box("Arbol_1", -8f, 4f, 1.5f, 1.5f, 2.2f, "Arbol");
            Box("Arbol_2", 8f, 2f, 1.5f, 1.5f, 2.2f, "Arbol");
            Box("Arbol_3", -6f, -5f, 1.5f, 1.5f, 2.2f, "Arbol");
            Label3D("EL SPOT", new Vector3(0f, 2.5f, 9f), new Color(1f, 0.85f, 0.5f), 0.12f);

            GameObject player = Player(new Vector3(0f, 1f, -8f));
            CreateCamera(player.transform, null);
            GameObject novia = Npc("Novia", new Vector3(-1.5f, 1f, -9.5f), "Novia", true);
            GameObject amigo = Npc("Amigo", new Vector3(1.5f, 1f, -9.5f), "Amigo", true);
            novia.GetComponent<CompanionFollower>().Configure(true, new Vector3(-1.2f, 0f, -1.2f));
            amigo.GetComponent<CompanionFollower>().Configure(true, new Vector3(1.2f, 0f, -1.4f));
            novia.AddComponent<RouteOnly>().Configure(Route.Bien);
            amigo.AddComponent<RouteOnly>().Configure(Route.Bien);

            GameObject spot = Marker("Spot", 0f, 5f);
            Objective("Objetivo_Spot", "Llega al mirador del Spot", spot.transform, true);
            NarrativeSequence good = Sequence("Intro_Bien", true, L("Novia", "Ya casi amanece. Vamos al mirador."));
            good.gameObject.AddComponent<RouteOnly>().Configure(Route.Bien);
            NarrativeSequence bad = Sequence("Intro_Mal", true,
                L("Pensamiento", "El Spot. Nadie más. Solo yo y lo que traigo en la bolsa."));
            bad.gameObject.AddComponent<RouteOnly>().Configure(Route.Mal);

            var ending = new GameObject("EndingController").AddComponent<EndingController>();
            ending.Configure(sun);
            On(Zone("Llegada_Spot", 0f, 5f, 8f, 3f, "").OnPlayerEnter, ending.Begin);
            Save(SceneNames.Spot);
        }

        // =====================================================================
        // FÁBRICA DE OBJETOS
        // =====================================================================

        private static void NewScene()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            counter = 0;
        }

        private static void Save(string sceneName)
        {
            FinishScene(sceneName);
            Scene scene = SceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(scene, SceneDir + "/" + sceneName + ".unity");
        }

        private static void Night()
        {
            Lighting(new Color(0.45f, 0.55f, 0.9f), 0.28f, new Color(0.2f, 0.22f, 0.36f), new Vector3(55f, -30f, 0f));
        }

        private static Light Lighting(Color sunColor, float intensity, Color ambient, Vector3 euler)
        {
            var go = new GameObject("Luz_Direccional");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = sunColor;
            light.intensity = intensity;
            light.shadows = LightShadows.Soft;
            go.transform.rotation = Quaternion.Euler(euler);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = ambient;
            RenderSettings.skybox = null;
            RenderSettings.fog = false;
            return light;
        }

        private static Camera CreateCamera(Transform target, Vector3? fixedPosition)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.05f);
            cam.fieldOfView = 55f;
            go.AddComponent<AudioListener>();
            if (target != null)
            {
                go.transform.position = target.position + CamOffset;
                go.transform.rotation = CamRotation;
                AddCinemachineRig(cam, target);
            }
            else if (fixedPosition.HasValue)
            {
                go.transform.position = fixedPosition.Value;
            }
            return cam;
        }

        private static GameObject Ground(float cx, float cz, float sx, float sz, string mat, float lift = 0f)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = "Suelo_" + mat + (lift > 0f ? "_Franja" : "");
            g.transform.position = new Vector3(cx, -0.5f + lift, cz);
            g.transform.localScale = new Vector3(sx, 1f, sz);
            g.GetComponent<Renderer>().sharedMaterial = mats[mat];
            if (lift > 0f) UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());
            g.isStatic = true;
            return g;
        }

        private static GameObject Box(string name, float cx, float cz, float sx, float sz, float height, string mat)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = name;
            g.transform.position = new Vector3(cx, height * 0.5f, cz);
            g.transform.localScale = new Vector3(sx, height, sz);
            g.GetComponent<Renderer>().sharedMaterial = mats[mat];
            return g;
        }

        private static GameObject Prop(string name, Vector3 center, Vector3 size, string mat)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = name;
            g.transform.position = center;
            g.transform.localScale = size;
            g.GetComponent<Renderer>().sharedMaterial = mats[mat];
            return g;
        }

        private static void Bounds(float minX, float maxX, float minZ, float maxZ)
        {
            float cx = (minX + maxX) * 0.5f, cz = (minZ + maxZ) * 0.5f;
            float w = maxX - minX + 2f, d = maxZ - minZ + 2f;
            Box("Limite_N", cx, maxZ + 0.5f, w, 1f, 3f, "Edificio");
            Box("Limite_S", cx, minZ - 0.5f, w, 1f, 0.8f, "Pared");
            Box("Limite_O", minX - 0.5f, cz, 1f, d, 2f, "Pared");
            Box("Limite_E", maxX + 0.5f, cz, 1f, d, 2f, "Pared");
        }

        private static GameObject Player(Vector3 position)
        {
            var root = new GameObject("Jugador") { tag = "Player" };
            root.transform.position = position;
            var col = root.AddComponent<CapsuleCollider>();
            col.height = 2f;
            col.radius = 0.45f;
            var rb = root.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            Transform rig = AnimatedVisual(root.transform, out GameObject visual);
            GameObject body = Primitive(PrimitiveType.Capsule, "Cuerpo", rig, Vector3.zero, new Vector3(0.9f, 1f, 0.9f), "Jugador");
            body.transform.localScale = new Vector3(0.9f, 1f, 0.9f);
            Primitive(PrimitiveType.Cube, "Audifonos", rig, new Vector3(0f, 0.85f, 0f), new Vector3(1.05f, 0.18f, 0.25f), "Negro");
            var facing = new GameObject("Frente");
            facing.transform.SetParent(rig, false);
            Primitive(PrimitiveType.Cube, "Nariz", facing.transform, new Vector3(0f, 0.45f, 0.42f), new Vector3(0.35f, 0.2f, 0.3f), "Blanco");

            var movement = root.AddComponent<PlayerMovement>();
            movement.Configure(facing.transform, visual.transform);
            root.AddComponent<PlayerStealth>().Configure(visual);
            root.AddComponent<PlayerInteractor>();
            root.AddComponent<PlayerRespawn>();
            return root;
        }

        private static GameObject Npc(string name, Vector3 position, string mat, bool companion)
        {
            var root = new GameObject(name);
            root.transform.position = position;
            var col = root.AddComponent<CapsuleCollider>();
            col.height = 2f;
            col.radius = 0.45f;
            var rb = root.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.isKinematic = !companion;
            Transform rig = AnimatedVisual(root.transform, out _);
            Primitive(PrimitiveType.Capsule, "Cuerpo", rig, Vector3.zero, new Vector3(0.9f, 1f, 0.9f), mat);
            Primitive(PrimitiveType.Cube, "Nariz", rig, new Vector3(0f, 0.45f, 0.42f), new Vector3(0.35f, 0.2f, 0.3f), "Blanco");
            Label3D(name, new Vector3(0f, 1.6f, 0f), Color.white, 0.05f, root.transform);
            if (companion) root.AddComponent<CompanionFollower>();
            return root;
        }

        private static GameObject Police(string name, Vector3[] route, bool pingPong, float speed, float wait)
        {
            Vector3 start = route[0] + Vector3.up;
            GameObject root = Npc(name, start, "Policia", false);
            Vector3 dir = (route.Length > 1 ? route[1] - route[0] : Vector3.forward);
            dir.y = 0f;
            root.transform.rotation = Quaternion.LookRotation(dir.normalized);

            var points = new Vector3[route.Length];
            for (int i = 0; i < route.Length; i++) points[i] = route[i] + Vector3.up;
            root.AddComponent<PatrolRoute>().Configure(points, speed, wait, pingPong, true, navMesh: true);

            var coneGo = new GameObject("Cono_Vision");
            coneGo.transform.SetParent(root.transform, false);
            var filter = coneGo.AddComponent<MeshFilter>();
            var renderer = coneGo.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = mats["Cono"];
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            var cone = root.AddComponent<PoliceVisionCone>();
            cone.Configure(7f, 70f, filter);
            cone.SetCaughtMessage("¡Un policía te vio!");
            GameObject alert = Label3D("!", new Vector3(0f, 2.4f, 0f), new Color(1f, 0.15f, 0.1f), 0.2f, root.transform);
            alert.name = "Alerta";
            cone.SetAlertIndicator(alert);
            return root;
        }

        private static GameObject Searchlight(string name, Vector3[] route, float radius)
        {
            var root = new GameObject(name);
            root.transform.position = route[0] + Vector3.up * 0.06f;
            GameObject disc = Primitive(PrimitiveType.Cylinder, "Circulo", root.transform, Vector3.zero,
                new Vector3(radius * 2f, 0.01f, radius * 2f), "Foco");
            disc.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            var lightGo = new GameObject("Luz_Foco");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 9f, 0f);
            lightGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var spot = lightGo.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.range = 14f;
            spot.spotAngle = 40f;
            spot.intensity = 12f;
            spot.color = new Color(1f, 0.97f, 0.85f);

            var points = new Vector3[route.Length];
            for (int i = 0; i < route.Length; i++) points[i] = route[i] + Vector3.up * 0.06f;
            root.AddComponent<PatrolRoute>().Configure(points, 2.5f, 0.3f, true, false);
            var detector = root.AddComponent<SearchlightDetector>();
            detector.Configure(radius, disc.GetComponent<Renderer>());
            detector.SetCaughtMessage("¡El foco del helicóptero te iluminó!");
            return root;
        }

        private static ChaserNPC Chaser(string name, Vector3 position, string mat, float speed, float activation, float leash,
                                        string caught, string thought, bool onlyByEvent, bool reactivate)
        {
            GameObject root = Npc(name, position, mat, false);
            var rb = root.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            var chaser = root.AddComponent<ChaserNPC>();
            chaser.Configure(speed, activation, leash, caught, thought, onlyByEvent, reactivate);
            return chaser;
        }

        private static TriggerZone Zone(string name, float cx, float cz, float sx, float sz, string thought)
        {
            var go = new GameObject(name);
            go.transform.position = new Vector3(cx, 1f, cz);
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(sx, 3f, sz);
            var zone = go.AddComponent<TriggerZone>();
            zone.Configure(true, thought);
            return zone;
        }

        private static void Checkpoint(string name, float cx, float cz, float depth)
        {
            var go = new GameObject(name);
            go.transform.position = new Vector3(cx, 1f, cz);
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(1.5f, 3f, depth);
            GameObject marker = Primitive(PrimitiveType.Cylinder, "Marca", go.transform, new Vector3(0f, -0.97f, 0f),
                new Vector3(1.2f, 0.02f, 1.2f), "Checkpoint");
            go.AddComponent<Checkpoint>().Configure(marker.GetComponent<Renderer>());
        }

        private static LevelExit Exit(string name, float cx, float cz, float sx, float sz, string scene, bool locked, string lockedMsg)
        {
            var go = new GameObject(name);
            go.transform.position = new Vector3(cx, 1f, cz);
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(sx, 3f, sz);
            GameObject marker = Primitive(PrimitiveType.Cube, "Marca", go.transform, new Vector3(0f, -0.97f, 0f),
                new Vector3(sx, 0.04f, sz), "Salida");
            var exit = go.AddComponent<LevelExit>();
            exit.Configure(scene, locked, lockedMsg, marker.GetComponent<Renderer>());
            Label3D("SALIDA", new Vector3(cx, 2f, cz), new Color(0.6f, 1f, 0.6f), 0.07f);
            return exit;
        }

        private static GameObject Marker(string name, float x, float z)
        {
            var go = new GameObject(name);
            go.transform.position = new Vector3(x, 1f, z);
            return go;
        }

        private static void Graffiti(string name, float x, float z, string text, string thought, float width = 3f, float depth = 0.15f)
        {
            GameObject panel = Box(name, x, z, width, depth, 1.6f, "Grafiti");
            panel.AddComponent<Graffiti>().Configure(text, thought);
            Label3D(text, new Vector3(x, 2.3f, z - 0.3f), new Color(1f, 0.45f, 0.9f), 0.06f);
        }

        private static void Pickup(string name, float x, float z)
        {
            GameObject g = Prop(name, new Vector3(x, 0.6f, z), new Vector3(0.5f, 0.7f, 0.35f), "Bateria");
            var col = g.GetComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(3f, 3f, 3f);
            g.AddComponent<BatteryPickup>();
        }

        private static void Container(string name, float x, float z)
        {
            GameObject g = Box(name, x, z, 2.4f, 1.4f, 1.4f, "Contenedor");
            g.AddComponent<HidingSpot>();
        }

        private static void Lamp(float x, float z, Color? color = null)
        {
            GameObject pole = Box("Poste", x, z, 0.2f, 0.2f, 3.5f, "Negro");
            UnityEngine.Object.DestroyImmediate(pole.GetComponent<Collider>());
            var lightGo = new GameObject("Luz_Poste");
            lightGo.transform.position = new Vector3(x, 3.6f, z);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 11f;
            light.intensity = 4f;
            light.color = color ?? new Color(1f, 0.78f, 0.45f);
            light.shadows = LightShadows.None;
        }

        private static ObjectiveStep Objective(string name, string text, Transform target, bool onStart)
        {
            var step = new GameObject(name).AddComponent<ObjectiveStep>();
            step.Configure(text, target, onStart);
            return step;
        }

        private static NarrativeSequence Sequence(string name, bool onStart, params DialogueLine[] lines)
        {
            var seq = new GameObject("Narrativa_" + name).AddComponent<NarrativeSequence>();
            seq.Configure(lines, onStart);
            return seq;
        }

        private static GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 scale, string mat)
        {
            GameObject g = GameObject.CreatePrimitive(type);
            g.name = name;
            UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());
            g.transform.SetParent(parent, false);
            g.transform.localPosition = localPos;
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = mats[mat];
            return g;
        }

        private static GameObject Label3D(string text, Vector3 position, Color color, float size, Transform parent = null)
        {
            var go = new GameObject("Texto_" + text);
            if (parent != null) go.transform.SetParent(parent, false);
            if (parent != null) go.transform.localPosition = position;
            else go.transform.position = position;
            go.transform.rotation = CamRotation;
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.font = font;
            tm.fontSize = 64;
            tm.characterSize = size;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = font.material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            if (parent != null) go.AddComponent<Billboard>();
            return go;
        }

        private static DialogueLine L(string speaker, string text) => new DialogueLine(speaker, text);
        private static Vector3 V(float x, float z) => new Vector3(x, 0f, z);

        private static void On(UnityEvent evt, UnityAction action) => UnityEventTools.AddPersistentListener(evt, action);
        private static void OnBool(UnityEvent evt, UnityAction<bool> action, bool value) =>
            UnityEventTools.AddBoolPersistentListener(evt, action, value);

        // =====================================================================
        // MATERIALES Y DATOS
        // =====================================================================

        private static void CreateMaterials()
        {
            mats = new Dictionary<string, Material>();
            Lit("Asfalto", new Color(0.2f, 0.2f, 0.24f));
            Lit("Banqueta", new Color(0.36f, 0.35f, 0.38f));
            Lit("Pasto", new Color(0.16f, 0.32f, 0.17f));
            Lit("Madera", new Color(0.42f, 0.3f, 0.22f));
            Lit("Pared", new Color(0.5f, 0.47f, 0.52f));
            Lit("Edificio", new Color(0.3f, 0.32f, 0.45f));
            Lit("Punto", new Color(0.35f, 0.18f, 0.18f));
            Lit("Puerta", new Color(0.45f, 0.28f, 0.12f));
            Lit("Cama", new Color(0.3f, 0.35f, 0.6f));
            Lit("Escritorio", new Color(0.5f, 0.36f, 0.24f));
            Lit("Celular", new Color(0.1f, 0.9f, 1f), true);
            Lit("Jugador", new Color(0.15f, 0.3f, 0.85f));
            Lit("Negro", new Color(0.05f, 0.05f, 0.06f));
            Lit("Blanco", new Color(0.95f, 0.95f, 0.95f));
            Lit("Policia", new Color(0.82f, 0.86f, 0.95f));
            Lit("Asaltante", new Color(0.85f, 0.15f, 0.12f));
            Lit("Orlando", new Color(0.2f, 0.75f, 0.3f));
            Lit("Novia", new Color(0.6f, 0.3f, 0.8f));
            Lit("Amigo", new Color(0.15f, 0.75f, 0.75f));
            Lit("Contenedor", new Color(0.12f, 0.4f, 0.22f));
            Lit("Auto", new Color(0.55f, 0.55f, 0.62f));
            Lit("Arbol", new Color(0.1f, 0.4f, 0.15f));
            Lit("Grafiti", new Color(0.85f, 0.2f, 0.7f), true);
            Lit("Bateria", new Color(0.2f, 0.85f, 1f), true);
            Lit("Checkpoint", new Color(0.2f, 0.45f, 1f), true);
            Lit("Salida", new Color(0.2f, 0.9f, 0.35f), true);
            Unlit("Cono", Color.white);
            Unlit("Foco", new Color(1f, 1f, 0.75f, 0.3f));
        }

        private static void Lit(string name, Color color, bool emissive = false)
        {
            Material m = LoadOrCreate(name, "Universal Render Pipeline/Lit");
            m.SetColor("_BaseColor", color);
            if (emissive)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", color * 0.8f);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            EditorUtility.SetDirty(m);
            mats[name] = m;
        }

        private static void Unlit(string name, Color color)
        {
            // Sprites/Default: transparente y respeta colores de vértice (cono dinámico).
            Material m = LoadOrCreate(name, "Sprites/Default");
            m.color = color;
            EditorUtility.SetDirty(m);
            mats[name] = m;
        }

        private static Material LoadOrCreate(string name, string shaderName)
        {
            string path = MatDir + "/" + name + ".mat";
            Shader shader = Shader.Find(shaderName);
            if (shader == null) throw new InvalidOperationException("No se encontró el shader " + shaderName);
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, path);
            }
            else if (m.shader != shader)
            {
                m.shader = shader;
            }
            return m;
        }

        private static void EnsureConfig()
        {
            const string path = "Assets/Resources/GameConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<GameConfig>(path) != null) return;
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<GameConfig>(), path);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}

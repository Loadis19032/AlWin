using AlWin;
using Swed64;
using System.Numerics;
using System.Runtime.InteropServices;

Swed swed = new Swed("cs2");

IntPtr client = swed.GetModuleBase("client.dll");

Renderer renderer = new Renderer();
Thread renderThread = new Thread(new ThreadStart(renderer.Start().Wait));
renderThread.Start();

Vector2 screenSize = renderer.screenSize;

List<Entity> entities = new List<Entity>();
Entity localPlayer = new Entity();


// offsets
int dwEntityList = 0x1A146E8;
int dwViewMatrix = 0x1A7F610;
int dwLocalPlayerPawn = 0x1868CC8;
int dwViewAngles = 0x1A89710;
//int m_entitySpottedState = 0x23D0;

// client
int m_vOldOrigin = 0x1324;
int m_iTeamNum = 0x3E3;
int m_lifeState = 0x348;
int m_hPlayerPawn = 0x80C;
int m_vecViewOffset = 0xCB0;
int m_iHealth = 0x344;
int m_entitySpottedState = 0xFA0; // CSPlayerPawnBase
int m_bSpotted = 0x8;
int m_iszPlayerName = 0x660;
int m_modelState = 0x170;
int m_pGameSceneNode = 0x328;
int m_pCameraServices = 0x11E0;
int m_bIsScoped = 0x23E8;
int m_iFOV = 0x210;


while (true)
{
    entities.Clear();

    

    IntPtr entityList = swed.ReadPointer(client, dwEntityList);
    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);
    IntPtr localPlayerPawn = swed.ReadPointer(client, dwLocalPlayerPawn);
    IntPtr cameraServices = swed.ReadPointer(client, m_pCameraServices);

    uint currentFov = swed.ReadUInt(cameraServices + m_iFOV);

    bool isScoped = swed.ReadBool(localPlayerPawn, m_bIsScoped);
    uint deFov = (uint)renderer.fov;

    

    if (!isScoped && currentFov != deFov)
    {
        swed.WriteUInt(cameraServices + m_iFOV, deFov);
    }

    localPlayer.team = swed.ReadInt(localPlayerPawn, m_iTeamNum);
    localPlayer.pawnAddress = swed.ReadPointer(client, dwLocalPlayerPawn);
    localPlayer.team = swed.ReadInt(localPlayer.pawnAddress, m_iTeamNum);
    localPlayer.origin = swed.ReadVec(localPlayer.pawnAddress, m_vOldOrigin);
    localPlayer.view = swed.ReadVec(localPlayer.pawnAddress, m_vecViewOffset);

    for (int i = 0; i < 65; i++)
    {
        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78);
        if (currentController == IntPtr.Zero) continue;

        int pawnHandle = swed.ReadInt(currentController, m_hPlayerPawn);
        if (pawnHandle == 0) continue;

        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);
        if (listEntry2 == IntPtr.Zero) continue;

        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));
        if (currentPawn == IntPtr.Zero) continue;

        bool spotted = swed.ReadBool(currentPawn, m_entitySpottedState + m_bSpotted);

        string spottedStatus = spotted == true ? "spotted" : " ";

        Console.WriteLine($"{localPlayer.name}: {spottedStatus}");

        uint lifestate = swed.ReadUInt(currentPawn, m_lifeState);
        if (lifestate != 256) continue;

        int health = swed.ReadInt(currentPawn, m_iHealth);
        int team = swed.ReadInt(currentPawn, m_iTeamNum);

        if (team == localPlayer.team && !renderer.enableOnTeam) 
            continue;

        float[] viewMatrix = swed.ReadMatrix(client + dwViewMatrix);

        IntPtr sceneNode = swed.ReadPointer(currentPawn, m_pGameSceneNode);
        IntPtr boneMatrix = swed.ReadPointer(sceneNode, m_modelState + 0x80);

        if (renderer.RadarHack)
            swed.WriteBool(currentPawn, m_entitySpottedState + m_bSpotted, true);

        Entity entity = new Entity();

        entity.pawnAddress = currentPawn;
        entity.controllerAddress = currentController;
        entity.lifeState = lifestate;
        entity.origin = swed.ReadVec(currentPawn, m_vOldOrigin);
        entity.view = swed.ReadVec(currentPawn, m_vecViewOffset);
        entity.name = swed.ReadString(currentController, m_iszPlayerName, 16).Split("\0")[0];
        entity.team = swed.ReadInt(currentPawn, m_iTeamNum);
        entity.health = swed.ReadInt(currentPawn, m_iHealth);
        entity.spotted = swed.ReadBool(currentPawn, m_entitySpottedState + m_bSpotted);
        entity.position = swed.ReadVec(currentPawn, m_vOldOrigin);
        entity.viewOffset = swed.ReadVec(currentPawn, m_vecViewOffset);
        entity.position2D = Calculate.WorldToScreen(viewMatrix, entity.position, screenSize);
        entity.viewPosition2D = Calculate.WorldToScreen(viewMatrix, Vector3.Add(entity.position, entity.viewOffset), screenSize);
        entity.distance = Vector3.Distance(entity.position, localPlayer.position);
        entity.bones = Calculate.ReadBones(boneMatrix, swed);
        entity.bones2d = Calculate.ReadBones2d(entity.bones, viewMatrix, screenSize);

        ViewMatrix viewmatrix = ReadMatrix(client + dwViewMatrix);
        //entity.head2d = Calculate.WorldToScreen(viewmatrix, entity.head2d);
        entities.Add(entity);

        Console.WriteLine($"{entity.health}hp, distance: {(int)(entity.distance) / 100}m");



    }

    entities = entities.OrderBy(o => o.distance).ToList();

    if (entities.Count > 0 && GetAsyncKeyState(0x58) < 0 && renderer.enableAimBot)
    {
        Vector3 playerView = Vector3.Add(localPlayer.origin, localPlayer.view);
        Vector3 entityView = Vector3.Add(entities[0].origin, entities[0].view);
    
        Vector2 newAngles = Calculate.CalculatingAngles(playerView, entityView);
        Vector3 newAnglesVec3 = new Vector3(newAngles.Y, newAngles.X, 0.0f);

        swed.WriteVec(client, dwViewAngles, newAnglesVec3);
    
    }

    renderer.UpdateLocalPlayer(localPlayer);
    renderer.UpdateEntities(entities);

    Thread.Sleep(1);
}

[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey);

ViewMatrix ReadMatrix(IntPtr matrixAddress)
{
    var viewMatrix = new ViewMatrix();
    var matrix = swed.ReadMatrix(matrixAddress);

    viewMatrix.m11 = matrix[0];
    viewMatrix.m12 = matrix[1];
    viewMatrix.m13 = matrix[2];
    viewMatrix.m14 = matrix[3];

    viewMatrix.m21 = matrix[4];
    viewMatrix.m22 = matrix[5];
    viewMatrix.m23 = matrix[6];
    viewMatrix.m24 = matrix[7];

    viewMatrix.m31 = matrix[8];
    viewMatrix.m32 = matrix[9];
    viewMatrix.m33 = matrix[10];
    viewMatrix.m34 = matrix[11];

    viewMatrix.m41 = matrix[12];
    viewMatrix.m42 = matrix[13];
    viewMatrix.m43 = matrix[14];
    viewMatrix.m44 = matrix[15];

    return viewMatrix;
}
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;

[CustomEditor(typeof(Grid))]
public class GridEditor : Editor {

    Grid grid;

    int oldindex = 0;

    void OnEnable()
    {
        grid = (Grid)target;
    }

    [MenuItem("Assets/Create/TileSet")]
    static void CreateTileSet()
    {
        var asset = ScriptableObject.CreateInstance<TileSet>();//에셋
        var path = AssetDatabase.GetAssetOrScenePath(Selection.activeObject);//에셋경로

        if(string.IsNullOrEmpty(path))//path가 비어있다면 경로를 기본 에셋폴더로 바꿈
        {
            path = "Assets";
        }
        else if(Path.GetExtension(path) != "")//path가 안 비었다면 파일이름찾아서 있으면 지우기
        {
            path = path.Replace(Path.GetFileName(path), "");
        }
        else
        {
            path += "/";//파일이름뒤에 / 추가
        }

        var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path+"TileSet.asset");
        AssetDatabase.CreateAsset(asset, assetPathAndName);//위의 결과로 에셋생성
        AssetDatabase.SaveAssets();//저장
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        asset.hideFlags = HideFlags.DontSave;

    }

    public override void OnInspectorGUI()//인스펙터에서
    {
        //base.OnInspectorGUI();

        grid.widht = createSlider("Grid Width", grid.widht);
        grid.height = createSlider("Grid height", grid.height);

        if (GUILayout.Button("Open Grid Window"))
        {
            GridWindow window = (GridWindow)EditorWindow.GetWindow(typeof(GridWindow));
            window.init();
        }

        grid.zOrder = (int)Mathf.Floor(createSlider("Order in Layer", grid.zOrder));

        //타일 프리팹
        EditorGUI.BeginChangeCheck();
        var newTileprefap = (Transform)EditorGUILayout.ObjectField("Tile prefap", grid.tileprefab, typeof(Transform), false);
        if(EditorGUI.EndChangeCheck())
        {
            grid.tileprefab = newTileprefap;
            Undo.RecordObject(target, "Grid Changed");
        }

        //타일맵
        EditorGUI.BeginChangeCheck();
        var newTileSet = (TileSet)EditorGUILayout.ObjectField("TileSet", grid.tileSet, typeof(TileSet), false);
        if(EditorGUI.EndChangeCheck())
        {
            grid.tileSet = newTileSet;
            Undo.RecordObject(target, "Grid Changed");
        }
        
        if(grid.tileSet != null)//타일셋이 비어있지 않다면 타일셋의 목록을 만듬
        {
            EditorGUI.BeginChangeCheck();
            var names = new string[grid.tileSet.prefaps.Length];
            var values = new int[names.Length];

            for(int i=0;i<names.Length;i++)
            {
                names[i] = grid.tileSet.prefaps[i] != null ? grid.tileSet.prefaps[i].name : "";
                values[i] = i;
            }
            var index = EditorGUILayout.IntPopup("Select Tile",oldindex,names,values);

            if(EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Grid Changed");
                if(oldindex != index)//리스트에서 선택된 프리팹이 이전과 다르다면 바꾸기
                {
                    oldindex = index;
                    grid.tileprefab = grid.tileSet.prefaps[index];

                    float width = grid.tileprefab.GetComponent<Renderer>().bounds.size.x;
                    float height = grid.tileprefab.GetComponent<Renderer>().bounds.size.y;

                    grid.widht = width;
                    grid.height = height;
                }
            }
            //선택된 타일 프리뷰
            GUILayout.BeginHorizontal();
            GUILayout.Label("Preview");
            Texture2D myTexture = AssetPreview.GetAssetPreview((Object)grid.tileprefab.gameObject);
            GUILayout.Label(myTexture);
            GUILayout.EndHorizontal();
        }
    }

    private float createSlider(string lablename,float sliderScale)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(lablename);
        sliderScale = EditorGUILayout.Slider(sliderScale, 1.0f, 100.0f, null);
        GUILayout.EndHorizontal();

        return sliderScale;
    }

    void OnSceneGUI()//씬에서 
    {
        int controlld = GUIUtility.GetControlID(FocusType.Passive);
        Event e = Event.current;
        Ray ray = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x,-e.mousePosition.y+Camera.current.pixelHeight));//카메라 기준으로의 터치 레이
        Vector3 mousePos = ray.origin;//마우스 포지션 뽑아내기

        if((e.isMouse && e.type ==EventType.MouseDown && e.button ==0) || (e.isMouse && e.type == EventType.MouseDrag && e.button == 3))//마우스가 클릭 되었다면 혹은 드래그
        {
            GUIUtility.hotControl = controlld;
            e.Use();

            GameObject gameobj;
            Transform prefap = grid.tileprefab;

            if(prefap)
            {
                Undo.IncrementCurrentGroup();
                Vector3 aligned = new Vector3(Mathf.Floor(mousePos.x / grid.widht) * grid.widht + grid.widht / 2.0f, Mathf.Floor(mousePos.y / grid.height) * grid.height + grid.height / 2.0f);
                Transform transform = GetTransformFromPos(aligned);
                if (transform != null)
                {
                    DestroyImmediate(transform.gameObject);
                }
                gameobj = (GameObject)PrefabUtility.InstantiatePrefab(prefap.gameObject);//게임오브젝트생성
                gameobj.transform.position = aligned;
                gameobj.GetComponent<SpriteRenderer>().sortingOrder = grid.zOrder;
                gameobj.name = grid.zOrder+"_"+ gameobj.name;
                gameobj.transform.parent = grid.transform;
                Undo.RegisterCreatedObjectUndo(gameobj, "Create" + gameobj.name);
            }
        }

        if(e.isMouse && e.type == EventType.MouseDown && e.button ==1)
        {
            GUIUtility.hotControl = controlld;
            e.Use();
            Vector3 aligned = new Vector3(Mathf.Floor(mousePos.x / grid.widht) * grid.widht + grid.widht / 2.0f, Mathf.Floor(mousePos.y / grid.height) * grid.height + grid.height / 2.0f);
            Transform transform = GetTransformFromPos(aligned);
            if(transform != null)
            {
                DestroyImmediate(transform.gameObject);
            }
        }

        if(e.isMouse && e.type == EventType.MouseUp)
        {
            GUIUtility.hotControl = 0;
        }
    }

    Transform GetTransformFromPos(Vector3 aligned)
    {
        int i = 0;
        while(i<grid.transform.childCount)
        {
            Transform transform = grid.transform.GetChild(i);
            if(transform.position == aligned)
            {
                if (transform.gameObject.GetComponent<SpriteRenderer>().sortingOrder == grid.zOrder)
                {
                    return transform;
                }
            }
            i++;
        }

        return null;
    }
}

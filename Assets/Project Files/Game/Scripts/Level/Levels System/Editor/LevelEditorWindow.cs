#pragma warning disable 649

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using Watermelon;
using System.Text;
using System.Collections.Generic;
using UnityEditorInternal;

namespace Watermelon.LevelEditor
{
    public class LevelEditorWindow : LevelEditorBase
    {

        //Path variables need to be changed ----------------------------------------
        private const string GAME_SCENE_PATH = "Assets/Project Files/Game/Scenes/Game.unity";
        private const string EDITOR_SCENE_PATH = "Assets/Project Files/Game/Scenes/Level Editor.unity";
        private static string EDITOR_SCENE_NAME = "Level Editor";

        //Window configuration
        private const string TITLE = "Level Editor";
        private const float WINDOW_MIN_WIDTH = 600;
        private const float WINDOW_MIN_HEIGHT = 560;

        //Level database fields
        private const string LEVELS_PROPERTY_NAME = "levels";
        private const string GATES_DATA_PROPERTY_NAME = "gatesData";
        private const string BOOSTERS_DATA_PROPERTY_NAME = "boostersData";
        private const string OBSTACLES_DATA_PROPERTY_NAME = "obstaclesData";
        private const string ENEMIES_DATA_PROPERTY_NAME = "enemiesData";
        private const string ROAD_PREFABS_PROPERTY_NAME = "roadsData";
        private const string FINISH_PREFABS_PROPERTY_NAME = "finishData";
        private const string ENVIRONMENT_PREFABS_PROPERTY_NAME = "environmentData";
        private const string DEFAULT_ENVIRONMENT_SPAWN_POSITION_Z_PROPERTY_NAME = "defaultEnvironmentSpawnPositionZ";
        private const string DEFAULT_CONTENT_SPAWN_POSITION_Z_PROPERTY_NAME = "defaultContentSpawnPositionZ";
        private const string DEFAULT_PLAYER_SPAWN_POSITION_Z_PROPERTY_NAME = "defaultPlayerSpawnPositionZ";
        private const string DEFAULT_CONTENT_OFFSET_Z_PROPERTY_NAME = "defaultContentOffsetZ";
        private const string DEFAULT_CAMERA_DATA_ID_PROPERTY_NAME = "defaultCameraDataId";
        private SerializedProperty levelsSerializedProperty;
        private SerializedProperty gatesDataProperty;
        private SerializedProperty boostersDataProperty;
        private SerializedProperty obstaclesDataProperty;
        private SerializedProperty enemiesDataProperty;
        private SerializedProperty roadPrefabsProperty;
        private SerializedProperty finishPrefabsProperty;
        private SerializedProperty environmentPrefabsProperty;
        private SerializedProperty defaultEnvironmentSpawnPositionZProperty;
        private SerializedProperty defaultContentSpawnPositionZProperty;
        private SerializedProperty defaultPlayerSpawnPositionZZProperty;
        private SerializedProperty defaultContentOffsetZProperty;
        private SerializedProperty defaultCameraDataIdProperty;


        private const string TYPE_PROPERTY_PATH = "type";
        private const string PREFAB_PROPERTY_PATH = "prefab";
        private const string STAT_PREFAB_PROPERTY_PATH = "statPrefab";
        private const string EXPLISIT_PREFAB_PROPERTY_PATH = "explicitPrefab";
        private const string EDITOR_TEXTURE_PROPERTY_PATH = "editorTexture";

        //TabHandler
        private TabHandler tabHandler;
        private const string LEVELS_TAB_NAME = "Levels";
        private const string ITEMS_TAB_NAME = "Items";

        //sidebar
        private LevelsHandler levelsHandler;
        private GUIStyle boxStyle;
        private LevelRepresentation selectedLevelRepresentation;
        private const int SIDEBAR_WIDTH = 240;
        private const string OPEN_GAME_SCENE_LABEL = "Open \"Game\" scene";

        private const string OPEN_GAME_SCENE_WARNING = "Please make sure you saved changes before swiching scene. Are you ready to proceed?";
        private const string REMOVE_SELECTION = "Remove selection";

        //General
        private const string YES = "Yes";
        private const string CANCEL = "Cancel";
        private const string WARNING_TITLE = "Warning";
        private SerializedProperty tempProperty;
        private string tempPropertyLabel;

        //PlayerPrefs
        private const string PREFS_LEVEL = "editor_level_index";
        private const string PREFS_WIDTH = "editor_sidebar_width";

        //rest of levels tab
        private const string ITEMS_LABEL = "Spawn items:";
        private const string FILE = "File";
        private const string COMPILING = "Compiling...";
        private const string ITEM_UNASSIGNED_ERROR = "Please assign prefab to this item in \"Items\"  tab.";
        private const string ITEM_ASSIGNED = "This buttton spawns item.";
        private const string TEST_LEVEL = "Test level";

        //Savable
        private const string POSITION_PROPERTY_PATH = "position";
        private const string ID_PROPERTY_PATH = "id";
        private const string HEALTH_PROPERTY_PATH = "health";
        private const string BOOSTER_HEIGHT_PROPERTY_PATH = "boosterHeight";
        private const string DAMAGE_HEALTH_DEPENDANT_PROPERTY_PATH = "damageHealthDependant";
        private const string DROP_TYPE_PROPERTY_PATH = "dropType";
        private const string DROP_CURRENCY_TYPE_PROPERTY_PATH = "dropCurrencyType";
        private const string DROP_ITEMS_COUNT_PROPERTY_PATH = "dropItemsCount";
        private const string DROP_ITEM_VALUE_PROPERTY_PATH = "dropItemValue";
        private const string GATE_TYPE_PROPERTY_PATH = "gateType";
        private const string STAT_TYPE_PROPERTY_PATH = "statType";
        private const string OPERATION_TYPE_PROPERTY_PATH = "operationType";
        private const string NUMERICAL_VALUE_PROPERTY_PATH = "numericalValue";
        private const string UPDATE_ON_HIT_PROPERTY_PATH = "updateOnHit";
        private const string STEP_PROPERTY_PATH = "step";
        private const string BOOSTER_TYPE_PROPERTY_PATH = "boosterType";
        private const string EXPLISIT_TYPE_PROPERTY_PATH = "explicitType";
        private const string EXPLICIT_ID_PROPERTY_PATH = "explicitId";
        private const string CHARACTERS_AMOUNT_PROPERTY_PATH = "charactersAmount";
        private const string ROAD_LENGTH_PROPERTY_PATH = "roadLength";
        private const string DAMAGE_PROPERTY_PATH = "damage";
        private const string FIRE_RATE_PROPERTY_PATH = "fireRate";
        private const string GUN_ID_PROPERTY_PATH = "gunId";
        private const string IS_NUMERICAL_PROPERTY_PATH = "isNumerical";

        //default values
        private const string DEFAULT_HEALTH_PROPERTY_PATH = "defaultHealth";
        private const string DEFAULT_BOOSTER_HEIGHT_PROPERTY_PATH = "defaultBoosterHeight";

        //other
        private const string PREVIEW_IMAGE_PROPERTY_PATH = "previewImage";
        private const string LENGTH_ALONG_Z_PROPERTY_PATH = "lengthAlongZ";

        private const float ITEMS_BUTTON_MAX_WIDTH = 120;
        private const float ITEMS_BUTTON_SPACE = 8;
        private const float ITEMS_BUTTON_WIDTH = 80;
        private const float ITEMS_BUTTON_HEIGHT = 80;
        private const string RENAME_LEVELS = "Rename Levels";
        private bool prefabAssigned;
        private GUIContent itemContent;
        private SerializedProperty currentLevelItemProperty;
        private Rect itemsListWidthRect;
        private SerializedProperty selectedItemProperty;
        private Vector2 levelItemsScrollVector;
        private float itemPosX;
        private float itemPosY;
        private Rect itemsRect;
        private Rect itemRect;
        private int itemsPerRow;
        private int rowCount;
        private GameObject tempGameobject;
        private string tempId;
        private Vector3 tempPosition;
        private Texture2D tempTexture;
        private int currentSideBarWidth;
        private LevelGenerator levelGenerator;
        private Rect separatorRect;
        private bool separatorIsDragged;
        private bool lastActiveLevelOpened;
        private float currentItemListWidth;
        private int selectedItemsTabTypeIndex;
        private int selectedSpawnOffsetTabindex;
        private int selectedEditingTabindex;
        private ItemType selectedItemType;
        private string[] itemsTabType = { "Gates", "Boosters", "Obstacles", "Enemies", "Finish", "Roads", "Environment" };
        private string[] spawnOffsetTabs = { "Disabled", "On last object", "With offset" };
        private string[] editingTabType = { "Manual", "Generator" };
        private ReorderableList tabList;
        private SerializedProperty currentTabItemProperty;

        private SerializedProperty previewProperty;
        private float spawnOffset;
        private GUIContent defaultTitleContent;
        private GUIContent modifiedTitleContent;
        private float zPosition;
        private Vector2 generatorScrollVector;
        private Texture2D infoIcon;
        private Color backupColor;

        protected override string LEVELS_DATABASE_FOLDER_PATH => "Assets/Project Files/Data/Level System";

        protected override WindowConfiguration SetUpWindowConfiguration(WindowConfiguration.Builder builder)
        {
            builder.KeepWindowOpenOnScriptReload(true);
            builder.SetWindowMinSize(new Vector2(WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT));
            return builder.Build();
        }

        public override bool WindowClosedInPlaymode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (window != null)
                {
                    window.Close();
                    OpenScene(GAME_SCENE_PATH);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        protected override Type GetLevelsDatabaseType()
        {
            return typeof(LevelsDatabase);
        }

        public override Type GetLevelType()
        {
            return typeof(LevelData);
        }

        protected override void ReadLevelDatabaseFields()
        {
            levelsSerializedProperty = levelsDatabaseSerializedObject.FindProperty(LEVELS_PROPERTY_NAME);
            gatesDataProperty = levelsDatabaseSerializedObject.FindProperty(GATES_DATA_PROPERTY_NAME);
            boostersDataProperty = levelsDatabaseSerializedObject.FindProperty(BOOSTERS_DATA_PROPERTY_NAME);
            obstaclesDataProperty = levelsDatabaseSerializedObject.FindProperty(OBSTACLES_DATA_PROPERTY_NAME);
            enemiesDataProperty = levelsDatabaseSerializedObject.FindProperty(ENEMIES_DATA_PROPERTY_NAME);
            roadPrefabsProperty = levelsDatabaseSerializedObject.FindProperty(ROAD_PREFABS_PROPERTY_NAME);
            finishPrefabsProperty = levelsDatabaseSerializedObject.FindProperty(FINISH_PREFABS_PROPERTY_NAME);
            environmentPrefabsProperty = levelsDatabaseSerializedObject.FindProperty(ENVIRONMENT_PREFABS_PROPERTY_NAME);
            defaultEnvironmentSpawnPositionZProperty = levelsDatabaseSerializedObject.FindProperty(DEFAULT_ENVIRONMENT_SPAWN_POSITION_Z_PROPERTY_NAME);
            defaultContentSpawnPositionZProperty = levelsDatabaseSerializedObject.FindProperty(DEFAULT_CONTENT_SPAWN_POSITION_Z_PROPERTY_NAME);
            defaultPlayerSpawnPositionZZProperty = levelsDatabaseSerializedObject.FindProperty(DEFAULT_PLAYER_SPAWN_POSITION_Z_PROPERTY_NAME);
            defaultContentOffsetZProperty = levelsDatabaseSerializedObject.FindProperty(DEFAULT_CONTENT_OFFSET_Z_PROPERTY_NAME);
            defaultCameraDataIdProperty = levelsDatabaseSerializedObject.FindProperty(DEFAULT_CAMERA_DATA_ID_PROPERTY_NAME);
        }

        protected override void InitializeVariables()
        {
            tabHandler = new TabHandler();
            tabHandler.AddTab(new TabHandler.Tab(LEVELS_TAB_NAME, DisplayLevelsTab));
            tabHandler.AddTab(new TabHandler.Tab("Settings", DisplayPropertiesTab));
            currentSideBarWidth = PlayerPrefs.GetInt(PREFS_WIDTH, SIDEBAR_WIDTH);
            levelGenerator = new LevelGenerator(levelsDatabase as LevelsDatabase, EditorUtils.GetAsset<LevelGeneratorDatabase>());
            Serializer.Init();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeAssemblyReload;
            spawnOffset = defaultContentOffsetZProperty.floatValue;
            defaultTitleContent = new GUIContent(DEFAULT_LEVEL_EDITOR_TITLE);
            modifiedTitleContent = new GUIContent(DEFAULT_LEVEL_EDITOR_TITLE + '*');
        }

        private void BeforeAssemblyReload()
        {
            SaveLevelIfPosssibleAndProceed(false);
            selectedLevelRepresentation = null;
            ClearScene();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != EDITOR_SCENE_NAME)
            {
                return;
            }

            if (change != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }

            if (levelsHandler.SelectedLevelIndex == -1)
            {
                OpenScene(GAME_SCENE_PATH);
            }
            else
            {
                TestLevel();
            }
        }

        private void OpenLastActiveLevel()
        {
            if (!lastActiveLevelOpened)
            {
                if (levelsSerializedProperty.arraySize > 0 && PlayerPrefs.HasKey(PREFS_LEVEL))
                {
                    levelsHandler.CustomList.SelectedIndex = Mathf.Clamp(PlayerPrefs.GetInt(PREFS_LEVEL, 0), 0, levelsSerializedProperty.arraySize - 1);
                    levelsHandler.OpenLevel(levelsHandler.SelectedLevelIndex);
                }

                lastActiveLevelOpened = true;
            }
        }

        protected override void Styles()
        {
            if (tabHandler != null)
            {
                tabHandler.SetDefaultToolbarStyle();
            }

            levelsHandler = new LevelsHandler(levelsDatabaseSerializedObject, levelsSerializedProperty);
            levelsHandler.removeElementCallback += HandleLevelRemove;

            boxStyle = new GUIStyle();
            Texture2D boxStyleTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            boxStyleTexture.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f, 1f));
            boxStyleTexture.Apply();
            boxStyle.border = new RectOffset(8, 8, 8, 8);
            boxStyle.normal.background = boxStyleTexture;
            infoIcon = EditorCustomStyles.GetIcon("icon_info");
        }

        private void HandleLevelRemove()
        {
            selectedLevelRepresentation = null;
        }

        public override void OpenLevel(UnityEngine.Object levelObject, int index)
        {
            SaveLevelIfPosssibleAndProceed(false);
            PlayerPrefs.SetInt(PREFS_LEVEL, index);
            PlayerPrefs.Save();
            selectedLevelRepresentation = new LevelRepresentation(levelObject);
            levelsHandler.UpdateCurrentLevelLabel(GetLevelLabel(levelObject, index));
            LoadLevelItems();
            levelGenerator.OnLevelOpened();
        }

        public override string GetLevelLabel(UnityEngine.Object levelObject, int index)
        {
            LevelRepresentation levelRepresentation = new LevelRepresentation(levelObject);
            return levelRepresentation.GetLevelLabel(index, stringBuilder);
        }

        public override void ClearLevel(UnityEngine.Object levelObject)
        {
            LevelRepresentation levelRepresentation = new LevelRepresentation(levelObject);
            levelRepresentation.Clear();
            levelRepresentation.playerSpawnPositionZProperty.floatValue = defaultPlayerSpawnPositionZZProperty.floatValue;
            levelRepresentation.cameraDataIdProperty.stringValue = defaultCameraDataIdProperty.stringValue;
            levelRepresentation.ApplyChanges();
        }

        protected override void DrawContent()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != EDITOR_SCENE_NAME)
            {
                DrawOpenEditorScene();
                return;
            }

            tabHandler.DisplayTab();
        }

        private void DrawOpenEditorScene()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox(EDITOR_SCENE_NAME + " scene required for level editor.", MessageType.Error, true);

            if (GUILayout.Button("Open \"" + EDITOR_SCENE_NAME + "\" scene"))
            {
                OpenScene(EDITOR_SCENE_PATH);
            }

            EditorGUILayout.EndVertical();
        }

        private void DisplayLevelsTab()
        {
            OpenLastActiveLevel();
            EditorGUILayout.BeginHorizontal();
            //sidebar 
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(currentSideBarWidth));
            levelsHandler.DisplayReordableList();
            DisplaySidebarButtons();
            EditorGUILayout.EndVertical();

            HandleChangingSideBar();

            //level content
            EditorGUILayout.BeginVertical(GUI.skin.box);
            DisplaySelectedLevel();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void HandleChangingSideBar()
        {
            separatorRect = EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(0), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndHorizontal();

            separatorRect.xMin -= GUI.skin.box.margin.right;
            separatorRect.xMax += GUI.skin.box.margin.left;

            EditorGUIUtility.AddCursorRect(separatorRect, MouseCursor.ResizeHorizontal);


            if (separatorRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    separatorIsDragged = true;
                    levelsHandler.IgnoreDragEvents = true;
                    Event.current.Use();
                }
            }

            if (separatorIsDragged)
            {
                if (Event.current.type == EventType.MouseUp)
                {
                    separatorIsDragged = false;
                    levelsHandler.IgnoreDragEvents = false;
                    PlayerPrefs.SetInt(PREFS_WIDTH, currentSideBarWidth);
                    PlayerPrefs.Save();
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseDrag)
                {
                    currentSideBarWidth = Mathf.RoundToInt(Event.current.delta.x) + currentSideBarWidth;
                    Event.current.Use();
                }
            }
        }

        private void DisplaySidebarButtons()
        {
            if (GUILayout.Button(RENAME_LEVELS, EditorCustomStyles.button))
            {
                if (SaveLevelIfPosssibleAndProceed())
                {
                    levelsHandler.RenameLevels();
                }
            }

            if (GUILayout.Button(OPEN_GAME_SCENE_LABEL, EditorCustomStyles.button))
            {
                if (SaveLevelIfPosssibleAndProceed())
                {
                    selectedLevelRepresentation = null;
                    levelsHandler.ClearSelection();
                    ClearScene();
                    OpenScene(GAME_SCENE_PATH);
                }
            }

            if (GUILayout.Button(REMOVE_SELECTION, EditorCustomStyles.button))
            {
                if (SaveLevelIfPosssibleAndProceed())
                {
                    selectedLevelRepresentation = null;
                    levelsHandler.ClearSelection();
                    ClearScene();
                }
            }
        }

        private static void ClearScene()
        {
            if (EditorSceneController.Instance != null)
                EditorSceneController.Instance.Clear();
        }

        private void SetAsCurrentLevel()
        {
            GlobalSave tempSave = SaveController.GetGlobalSave();
            tempSave.GetSaveObject<LevelNumberSave>("Level Number Save").SetLevelNumber(levelsHandler.SelectedLevelIndex);
            SaveController.SaveCustom(tempSave);
        }

        private void DisplaySelectedLevel()
        {
            if (levelsHandler.SelectedLevelIndex == -1)
            {
                return;
            }

            //handle level file field
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(levelsHandler.SelectedLevelProperty, new GUIContent(FILE));

            if (EditorGUI.EndChangeCheck())
            {
                levelsHandler.ReopenLevel();
            }

            if (selectedLevelRepresentation.NullLevel)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(selectedLevelRepresentation.noteProperty);

            if (EditorGUI.EndChangeCheck())
            {
                levelsHandler.UpdateCurrentLevelLabel(selectedLevelRepresentation.GetLevelLabel(levelsHandler.SelectedLevelIndex, stringBuilder));
            }

            EditorGUILayout.Space(-10f);

            selectedLevelRepresentation.DisplayProperties();
            EditorSceneController.Instance.PlayerSpawn.transform.position = EditorSceneController.Instance.PlayerSpawn.transform.position.SetZ(selectedLevelRepresentation.playerSpawnPositionZProperty.floatValue);
            selectedLevelRepresentation.ApplyChanges();
            DisplaySaveSection();
            EditorGUILayout.Space(5f);
            DisplayItemsListSection();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(TEST_LEVEL, GUILayout.Width(EditorGUIUtility.labelWidth), GUILayout.Height(30f)))
            {
                TestLevel();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DisplaySaveSection()
        {
            if (EditorSceneController.Instance.IsLevelChanged())
            {
                EditorGUILayout.Space(5f);
                backupColor = GUI.color;
                GUI.color = Color.red;

                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                GUI.color = backupColor;
                EditorGUILayout.LabelField(new GUIContent(infoIcon), GUILayout.MaxWidth(20));
                EditorGUILayout.LabelField("Level have some unsaved changes.");

                if (GUILayout.Button("Discard"))
                {
                    LoadLevelItems();
                }

                if (GUILayout.Button("Save"))
                {
                    SaveLevelItems();
                    EditorSceneController.Instance.RegisterLevelState();
                }

                titleContent = modifiedTitleContent;
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                titleContent = defaultTitleContent;
            }
        }

        private void TestLevel()
        {
            SaveLevelIfPosssibleAndProceed(false);
            SetAsCurrentLevel();
            OpenScene(GAME_SCENE_PATH);
            EditorApplication.isPlaying = true;
        }

        private void DisplayItemsListSection()
        {
            selectedEditingTabindex = GUILayout.Toolbar(selectedEditingTabindex, editingTabType, EditorCustomStyles.tab);

            //handle generator tab
            if (selectedEditingTabindex == 1)
            {
                generatorScrollVector = EditorGUILayout.BeginScrollView(generatorScrollVector);
                EditorGUILayout.BeginVertical();
                levelGenerator.DrawGUI();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
                return;
            }


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(5f); //move horizontaly
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(2); //move verticaly
            selectedItemsTabTypeIndex = GUILayout.SelectionGrid(selectedItemsTabTypeIndex, itemsTabType, 1);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorCustomStyles.windowSpacedContent);
            itemsListWidthRect = GUILayoutUtility.GetRect(1, Screen.width, 0, 0, GUILayout.ExpandWidth(true));
            levelItemsScrollVector = EditorGUILayout.BeginScrollView(levelItemsScrollVector);

            if (!(selectedItemType == ItemType.Road || selectedItemType == ItemType.Environment))// handle spawn offset
            {
                EditorGUILayout.BeginVertical(boxStyle);
                EditorGUILayout.LabelField("Spawn offset z", EditorCustomStyles.labelBold);
                EditorGUILayout.BeginHorizontal();
                selectedSpawnOffsetTabindex = GUILayout.Toolbar(selectedSpawnOffsetTabindex, spawnOffsetTabs, GUI.skin.button, GUI.ToolbarButtonSize.FitToContents, GUILayout.ExpandWidth(false));
                spawnOffset = EditorGUILayout.FloatField(spawnOffset, GUILayout.Width(50f));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10f);

            selectedItemType = (ItemType)selectedItemsTabTypeIndex;
            selectedItemProperty = GetItemTypeProperty(selectedItemType);

            if ((itemsListWidthRect.width > 1) && (Event.current.type == EventType.Repaint))
            {
                currentItemListWidth = itemsListWidthRect.width;
            }



            itemsRect = EditorGUILayout.BeginVertical();
            itemPosX = itemsRect.x;
            itemPosY = itemsRect.y;

            //assigning space
            if (selectedItemProperty.arraySize != 0)
            {
                itemsPerRow = Mathf.FloorToInt((currentItemListWidth - 16) / (ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH)); // 16- space for vertical scroll
                rowCount = Mathf.CeilToInt(selectedItemProperty.arraySize * 1f / itemsPerRow);
                GUILayout.Space(rowCount * (ITEMS_BUTTON_SPACE + ITEMS_BUTTON_HEIGHT));
            }

            for (int i = 0; i < selectedItemProperty.arraySize; i++)
            {
                tempProperty = selectedItemProperty.GetArrayElementAtIndex(i);
                tempGameobject = (GameObject)tempProperty.FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue;
                prefabAssigned = tempGameobject != null;

                if (prefabAssigned)
                {
                    previewProperty = tempProperty.FindPropertyRelative(PREVIEW_IMAGE_PROPERTY_PATH);

                    if (previewProperty != null && previewProperty.objectReferenceValue != null)
                    {
                        itemContent = new GUIContent((Texture2D)previewProperty.objectReferenceValue, tempGameobject.name);
                    }
                    else if (AssetPreview.GetAssetPreview(tempGameobject) == null)
                    {
                        if (AssetPreview.IsLoadingAssetPreview(tempGameobject.GetInstanceID()))
                        {
                            itemContent = new GUIContent(AssetPreview.GetAssetPreview(tempGameobject), tempGameobject.name);
                        }
                        else
                        {
                            itemContent = new GUIContent(AssetPreview.GetMiniThumbnail(tempGameobject), tempGameobject.name);
                        }
                    }
                    else
                    {
                        itemContent = new GUIContent(AssetPreview.GetAssetPreview(tempGameobject), tempGameobject.name);
                    }
                }
                else
                {
                    itemContent = new GUIContent("[NULL]", ITEM_UNASSIGNED_ERROR);
                }

                //check if need to start new row
                if (itemPosX + ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH > currentItemListWidth - 16)
                {
                    itemPosX = itemsRect.x;
                    itemPosY = itemPosY + ITEMS_BUTTON_HEIGHT + ITEMS_BUTTON_SPACE;
                }

                itemRect = new Rect(itemPosX, itemPosY, ITEMS_BUTTON_WIDTH, ITEMS_BUTTON_HEIGHT);

                EditorGUI.BeginDisabledGroup(!prefabAssigned);

                if (GUI.Button(itemRect, itemContent, EditorCustomStyles.button))
                {
                    tempId = tempProperty.FindPropertyRelative(ID_PROPERTY_PATH).stringValue;
                    tempPosition = Vector3.zero;
                    GameObject gameObject;

                    if (selectedItemType == ItemType.Road || selectedItemType == ItemType.Environment)
                    {
                        gameObject = EditorSceneController.Instance.SpawnPrefab(tempGameobject, tempPosition, false, true);
                    }
                    else
                    {
                        if (selectedSpawnOffsetTabindex == 0) //0 - Disabled
                        {
                            tempPosition = new Vector3(0, 0, defaultContentSpawnPositionZProperty.floatValue);
                        }
                        else if (selectedSpawnOffsetTabindex == 1)
                        {
                            if (EditorSceneController.Instance.Content.transform.childCount > 0)
                            {
                                tempPosition = new Vector3(0, 0, EditorSceneController.Instance.Content.transform.GetChild(0).transform.position.z);
                            }
                            else
                            {
                                tempPosition = new Vector3(0, 0, defaultContentSpawnPositionZProperty.floatValue);
                            }
                        }
                        else
                        {
                            if (EditorSceneController.Instance.Content.transform.childCount > 0)
                            {
                                tempPosition = new Vector3(0, 0, EditorSceneController.Instance.Content.transform.GetChild(0).transform.position.z + spawnOffset);
                            }
                            else
                            {
                                tempPosition = new Vector3(0, 0, spawnOffset);
                            }
                        }

                        gameObject = EditorSceneController.Instance.SpawnPrefab(tempGameobject, tempPosition, true, true);
                    }

                    if (selectedItemType == ItemType.Gates)
                    {
                        SavableGate savableGate = EditorSceneController.Instance.AddGate(gameObject, tempId);
                        savableGate.IsNumerical = tempProperty.FindPropertyRelative(IS_NUMERICAL_PROPERTY_PATH).boolValue;
                        savableGate.GateType = (GateType)tempProperty.FindPropertyRelative(GATE_TYPE_PROPERTY_PATH).intValue;
                        savableGate.SetDefaultID();
                    }
                    else if (selectedItemType == ItemType.Boosters)
                    {
                        SavableBooster savableBooster = EditorSceneController.Instance.AddBooster(gameObject, tempId);
                        savableBooster.IsNumerical = tempProperty.FindPropertyRelative(IS_NUMERICAL_PROPERTY_PATH).boolValue;
                        savableBooster.BoosterType = (BoosterType)tempProperty.FindPropertyRelative(BOOSTER_TYPE_PROPERTY_PATH).intValue;
                        savableBooster.SetDefaultID();
                    }
                    else if (selectedItemType == ItemType.Obstacles)
                    {
                        SavableObstacle savableObstacle = EditorSceneController.Instance.AddObstacle(gameObject, tempId);
                        savableObstacle.BoosterHeight = tempProperty.FindPropertyRelative(DEFAULT_BOOSTER_HEIGHT_PROPERTY_PATH).floatValue;
                        savableObstacle.Health = tempProperty.FindPropertyRelative(DEFAULT_HEALTH_PROPERTY_PATH).intValue;
                    }
                    else if (selectedItemType == ItemType.Enemies)
                    {
                        EditorSceneController.Instance.AddEnemy(gameObject, tempId);
                    }
                    else if (selectedItemType == ItemType.Finish)
                    {
                        EditorSceneController.Instance.AddFinish(gameObject, tempId);
                    }
                    else if (selectedItemType == ItemType.Road)
                    {
                        SavableRoad firstElement = EditorSceneController.Instance.Background.GetComponentInChildren<SavableRoad>(); // have max Z value because of sorting
                        SavableRoad savableRoad = EditorSceneController.Instance.AddRoad(gameObject, tempId);
                        zPosition = defaultEnvironmentSpawnPositionZProperty.floatValue;

                        if (firstElement != null)
                        {
                            for (int j = 0; j < selectedItemProperty.arraySize; j++)
                            {
                                if (selectedItemProperty.GetArrayElementAtIndex(j).FindPropertyRelative(ID_PROPERTY_PATH).stringValue.Equals(firstElement.Id))
                                {
                                    zPosition = firstElement.Position.z + selectedItemProperty.GetArrayElementAtIndex(j).FindPropertyRelative(LENGTH_ALONG_Z_PROPERTY_PATH).floatValue;
                                    break;
                                }
                            }
                        }

                        savableRoad.transform.position = savableRoad.transform.position.SetZ(zPosition);
                    }
                    else if (selectedItemType == ItemType.Environment)
                    {
                        SavableEnvironment firstElement = EditorSceneController.Instance.Background.GetComponentInChildren<SavableEnvironment>(); // have max Z value because of sorting
                        SavableEnvironment savableEnvironment = EditorSceneController.Instance.AddEnvironment(gameObject, tempId);
                        zPosition = defaultEnvironmentSpawnPositionZProperty.floatValue;

                        if (firstElement != null)
                        {
                            for (int j = 0; j < selectedItemProperty.arraySize; j++)
                            {
                                if (selectedItemProperty.GetArrayElementAtIndex(j).FindPropertyRelative(ID_PROPERTY_PATH).stringValue.Equals(firstElement.Id))
                                {
                                    zPosition = firstElement.Position.z + selectedItemProperty.GetArrayElementAtIndex(j).FindPropertyRelative(LENGTH_ALONG_Z_PROPERTY_PATH).floatValue;
                                    break;
                                }
                            }
                        }

                        savableEnvironment.transform.position = savableEnvironment.transform.position.SetZ(zPosition);
                    }
                }

                EditorGUI.EndDisabledGroup();

                itemPosX += ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH;
            }


            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void LoadLevelItems()
        {
            EditorSceneController.Instance.Clear();
            EditorSceneController.Instance.LevelLoaded = false;
            LoadGates();
            LoadBoosters();
            LoadObstacles();
            LoadEnemies();
            LoadRoads();
            LoadFinish();
            LoadEnvironment();
            EditorSceneController.Instance.RegisterLevelState();
            EditorSceneController.Instance.LevelLoaded = true; // optimization
        }



        private void LoadGates()
        {
            SerializedProperty currentProperty;
            string id;
            Vector3 position;
            GameObject tempGameObject;
            SavableGate tempSavable;
            GameObject prefab;

            for (int i = 0; i < selectedLevelRepresentation.gatesDataProperty.arraySize; i++)
            {
                currentProperty = selectedLevelRepresentation.gatesDataProperty.GetArrayElementAtIndex(i);
                id = currentProperty.FindPropertyRelative(ID_PROPERTY_PATH).stringValue;
                position = currentProperty.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value;
                prefab = GetPrefab(ItemType.Gates, id);

                if (prefab == null)
                {
                    continue;
                }

                tempGameObject = EditorSceneController.Instance.SpawnPrefab(prefab, position);
                tempSavable = EditorSceneController.Instance.AddGate(tempGameObject, id);
                tempSavable.GateType = (GateType)currentProperty.FindPropertyRelative(GATE_TYPE_PROPERTY_PATH).intValue;
                tempSavable.OperationType = (OperationType)currentProperty.FindPropertyRelative(OPERATION_TYPE_PROPERTY_PATH).intValue;
                tempSavable.NumericalValue = currentProperty.FindPropertyRelative(NUMERICAL_VALUE_PROPERTY_PATH).floatValue;
                tempSavable.UpdateOnHit = currentProperty.FindPropertyRelative(UPDATE_ON_HIT_PROPERTY_PATH).boolValue;
                tempSavable.Step = currentProperty.FindPropertyRelative(STEP_PROPERTY_PATH).floatValue;
                tempSavable.ExplicitId = currentProperty.FindPropertyRelative(EXPLICIT_ID_PROPERTY_PATH).stringValue;
                tempSavable.CharactersAmount = currentProperty.FindPropertyRelative(CHARACTERS_AMOUNT_PROPERTY_PATH).intValue;
                tempSavable.IsNumerical = IsNumerical(ItemType.Gates, id);
            }
        }

        private void SaveGates()
        {
            SavableGate[] savables = EditorSceneController.Instance.Content.GetComponentsInChildren<SavableGate>();
            selectedLevelRepresentation.gatesDataProperty.arraySize = savables.Length;
            SerializedProperty currentProperty;

            for (int i = 0; i < savables.Length; i++)
            {
                currentProperty = selectedLevelRepresentation.gatesDataProperty.GetArrayElementAtIndex(i);
                currentProperty.FindPropertyRelative(ID_PROPERTY_PATH).stringValue = savables[i].Id;
                currentProperty.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value = savables[i].Position;
                currentProperty.FindPropertyRelative(GATE_TYPE_PROPERTY_PATH).intValue = (int)savables[i].GateType;
                currentProperty.FindPropertyRelative(OPERATION_TYPE_PROPERTY_PATH).intValue = (int)savables[i].OperationType;
                currentProperty.FindPropertyRelative(NUMERICAL_VALUE_PROPERTY_PATH).floatValue = savables[i].NumericalValue;
                currentProperty.FindPropertyRelative(UPDATE_ON_HIT_PROPERTY_PATH).boolValue = savables[i].UpdateOnHit;
                currentProperty.FindPropertyRelative(STEP_PROPERTY_PATH).floatValue = savables[i].Step;
                currentProperty.FindPropertyRelative(EXPLICIT_ID_PROPERTY_PATH).stringValue = savables[i].ExplicitId;
                currentProperty.FindPropertyRelative(CHARACTERS_AMOUNT_PROPERTY_PATH).intValue = savables[i].CharactersAmount;
            }

        }

        private void LoadBoosters()
        {
            SerializedProperty currentProperty;
            string id;
            Vector3 position;
            GameObject tempGameObject;
            SavableBooster tempSavable;
            BoosterType boosterType;
            GameObject prefab;

            for (int i = 0; i < selectedLevelRepresentation.boostersDataProperty.arraySize; i++)
            {
                currentProperty = selectedLevelRepresentation.boostersDataProperty.GetArrayElementAtIndex(i);
                id = currentProperty.FindPropertyRelative(ID_PROPERTY_PATH).stringValue;
                position = currentProperty.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value;
                boosterType = (BoosterType)currentProperty.FindPropertyRelative(BOOSTER_TYPE_PROPERTY_PATH).intValue;
                prefab = GetPrefab(ItemType.Boosters, id);

                if (prefab == null)
                {
                    continue;
                }

                tempGameObject = EditorSceneController.Instance.SpawnPrefab(prefab, position);
                tempSavable = EditorSceneController.Instance.AddBooster(tempGameObject, id);
                tempSavable.BoosterType = boosterType;
                tempSavable.OperationType = (OperationType)currentProperty.FindPropertyRelative(OPERATION_TYPE_PROPERTY_PATH).intValue;
                tempSavable.NumericalValue = currentProperty.FindPropertyRelative(NUMERICAL_VALUE_PROPERTY_PATH).floatValue;
                if (boosterType == BoosterType.GiveCharacter)
                {
                    tempSavable.CharacterID = currentProperty.FindPropertyRelative(EXPLICIT_ID_PROPERTY_PATH).stringValue;
                }
                else
                {
                    tempSavable.WeaponID = currentProperty.FindPropertyRelative(EXPLICIT_ID_PROPERTY_PATH).stringValue;
                }
                tempSavable.CharactersAmount = currentProperty.FindPropertyRelative(CHARACTERS_AMOUNT_PROPERTY_PATH).intValue;
                tempSavable.IsNumerical = IsNumerical(ItemType.Boosters, id);
            }
        }

        private void SaveBoosters()
        {
            SavableBooster[] savables = EditorSceneController.Instance.Content.GetComponentsInChildren<SavableBooster>();
            selectedLevelRepresentation.boostersDataProperty.arraySize = savables.Length;
            SerializedProperty currentProperty;

            for (int i = 0; i < savables.Length; i++)
            {
                currentProperty = selectedLevelRepresentation.boostersDataProperty.GetArrayElementAtIndex(i);
                currentProperty.FindPropertyRelative(ID_PROPERTY_PATH).stringValue = savables[i].Id;
                currentProperty.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value = savables[i].Position;
                currentProperty.FindPropertyRelative(OPERATION_TYPE_PROPERTY_PATH).intValue = (int)savables[i].OperationType;
                currentProperty.FindPropertyRelative(NUMERICAL_VALUE_PROPERTY_PATH).floatValue = savables[i].NumericalValue;
                currentProperty.FindPropertyRelative(BOOSTER_TYPE_PROPERTY_PATH).intValue = (int)savables[i].BoosterType;
                currentProperty.FindPropertyRelative(EXPLICIT_ID_PROPERTY_PATH).stringValue = savables[i].BoosterType == BoosterType.GiveWeapon ? savables[i].WeaponID : savables[i].CharacterID;
                currentProperty.FindPropertyRelative(CHARACTERS_AMOUNT_PROPERTY_PATH).intValue = savables[i].CharactersAmount;
            }
        }

        private void LoadObstacles()
        {
            SerializedProperty currentProperty;
            string id;
            Vector3 position;
            GameObject tempGameObject;
            SavableObstacle tempSavable;
            GameObject prefab;

            for (int i = 0; i < selectedLevelRepresentation.obstaclesDataProperty.arraySize; i++)
            {
                currentProperty = selectedLevelRepresentation.obstaclesDataProperty.GetArrayElementAtIndex(i);
                id = currentProperty.FindPropertyRelative(ID_PROPERTY_PATH).stringValue;
                position = currentProperty.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value;
                prefab = GetPrefab(ItemType.Obstacles, id);

                if (prefab == null)
                {
                    continue;
                }

                tempGameObject = EditorSceneController.Instance.SpawnPrefab(prefab, position);
                tempSavable = EditorSceneController.Instance.AddObstacle(tempGameObject, id);
                tempSavable.Health = currentProperty.FindPropertyRelative(HEALTH_PROPERTY_PATH).intValue;
                tempSavable.BoosterHeight = currentProperty.FindPropertyRelative(BOOSTER_HEIGHT_PROPERTY_PATH).floatValue;
                tempSavable.DropType = (DropableItemType)currentProperty.FindPropertyRelative(DROP_TYPE_PROPERTY_PATH).intValue;
                tempSavable.DropCurrencyType = (CurrencyType)currentProperty.FindPropertyRelative(DROP_CURRENCY_TYPE_PROPERTY_PATH).intValue;
                tempSavable.DropItemsCount = currentProperty.FindPropertyRelative(DROP_ITEMS_COUNT_PROPERTY_PATH).intValue;
                tempSavable.DropItemValue = currentProperty.FindPropertyRelative(DROP_ITEM_VALUE_PROPERTY_PATH).floatValue;
            }
        }

        private void SaveObstacles()
        {
            SavableObstacle[] savables = EditorSceneController.Instance.Content.GetComponentsInChildren<SavableObstacle>();
            selectedLevelRepresentation.obstaclesDataProperty.arraySize = savables.Length;
            SerializedProperty currentProperty;

            for (int i = 0; i < savables.Length; i++)
            {
                currentProperty = selectedLevelRepresentation.obstaclesDataProperty.GetArrayElementAtIndex(i);
                currentProperty.FindPropertyRelative(ID_PROPERTY_PATH).stringValue = savables[i].Id;
                currentProperty.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value = savables[i].Position;
                currentProperty.FindPropertyRelative(HEALTH_PROPERTY_PATH).intValue = savables[i].Health;
                currentProperty.FindPropertyRelative(BOOSTER_HEIGHT_PROPERTY_PATH).floatValue = savables[i].BoosterHeight;
                currentProperty.FindPropertyRelative(DROP_TYPE_PROPERTY_PATH).intValue = (int)savables[i].DropType;
                currentProperty.FindPropertyRelative(DROP_CURRENCY_TYPE_PROPERTY_PATH).intValue = (int)savables[i].DropCurrencyType;
                currentProperty.FindPropertyRelative(DROP_ITEMS_COUNT_PROPERTY_PATH).intValue = savables[i].DropItemsCount;
                currentProperty.FindPropertyRelative(DROP_ITEM_VALUE_PROPERTY_PATH).floatValue = savables[i].DropItemValue;
            }
        }

        private void LoadEnemies()
        {
            SerializedProperty currentProperty;
            string id;
            Vector3 position;
            GameObject tempGameObject;
            SavableEnemy tempSavable;
            GameObject prefab;

            for (int i = 0; i < selectedLevelRepresentation.enemiesDataProperty.arraySize; i++)
            {
                currentProperty = selectedLevelRepresentation.enemiesDataProperty.GetArrayElementAtIndex(i);
                id = currentProperty.FindPropertyRelative(ID_PROPERTY_PATH).stringValue;
                position = currentProperty.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value;
                prefab = GetPrefab(ItemType.Enemies, id);

                if (prefab == null)
                {
                    continue;
                }

                tempGameObject = EditorSceneController.Instance.SpawnPrefab(prefab, position);
                tempSavable = EditorSceneController.Instance.AddEnemy(tempGameObject, id);
                tempSavable.Health = currentProperty.FindPropertyRelative(HEALTH_PROPERTY_PATH).floatValue;
                tempSavable.Damage = currentProperty.FindPropertyRelative(DAMAGE_PROPERTY_PATH).floatValue;
                tempSavable.FireRate = currentProperty.FindPropertyRelative(FIRE_RATE_PROPERTY_PATH).floatValue;
                tempSavable.GunId = currentProperty.FindPropertyRelative(GUN_ID_PROPERTY_PATH).stringValue;
                tempSavable.DropType = (DropableItemType)currentProperty.FindPropertyRelative(DROP_TYPE_PROPERTY_PATH).intValue;
                tempSavable.DropCurrencyType = (CurrencyType)currentProperty.FindPropertyRelative(DROP_CURRENCY_TYPE_PROPERTY_PATH).intValue;
                tempSavable.DropItemsCount = currentProperty.FindPropertyRelative(DROP_ITEMS_COUNT_PROPERTY_PATH).intValue;
                tempSavable.DropItemValue = currentProperty.FindPropertyRelative(DROP_ITEM_VALUE_PROPERTY_PATH).floatValue;
            }
        }

        private void SaveEnemies()
        {
            SavableEnemy[] savables = EditorSceneController.Instance.Content.GetComponentsInChildren<SavableEnemy>();
            selectedLevelRepresentation.enemiesDataProperty.arraySize = savables.Length;
            SerializedProperty currentProperty;

            for (int i = 0; i < savables.Length; i++)
            {
                currentProperty = selectedLevelRepresentation.enemiesDataProperty.GetArrayElementAtIndex(i);
                currentProperty.FindPropertyRelative(ID_PROPERTY_PATH).stringValue = savables[i].Id;
                currentProperty.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value = savables[i].Position;
                currentProperty.FindPropertyRelative(HEALTH_PROPERTY_PATH).floatValue = savables[i].Health;
                currentProperty.FindPropertyRelative(DAMAGE_PROPERTY_PATH).floatValue = savables[i].Damage;
                currentProperty.FindPropertyRelative(FIRE_RATE_PROPERTY_PATH).floatValue = savables[i].FireRate;
                currentProperty.FindPropertyRelative(GUN_ID_PROPERTY_PATH).stringValue = savables[i].GunId;
                currentProperty.FindPropertyRelative(DROP_TYPE_PROPERTY_PATH).intValue = (int)savables[i].DropType;
                currentProperty.FindPropertyRelative(DROP_CURRENCY_TYPE_PROPERTY_PATH).intValue = (int)savables[i].DropCurrencyType;
                currentProperty.FindPropertyRelative(DROP_ITEMS_COUNT_PROPERTY_PATH).intValue = savables[i].DropItemsCount;
                currentProperty.FindPropertyRelative(DROP_ITEM_VALUE_PROPERTY_PATH).floatValue = savables[i].DropItemValue;
            }
        }

        private void LoadRoads()
        {
            SerializedProperty currentProperty;
            string id;
            Vector3 position;
            GameObject tempGameObject;
            SavableRoad tempSavable;
            GameObject prefab;

            for (int i = 0; i < selectedLevelRepresentation.roadsDataProperty.arraySize; i++)
            {
                currentProperty = selectedLevelRepresentation.roadsDataProperty.GetArrayElementAtIndex(i);
                id = currentProperty.FindPropertyRelative(ID_PROPERTY_PATH).stringValue;
                position = currentProperty.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value;
                prefab = GetPrefab(ItemType.Road, id);

                if (prefab == null)
                {
                    continue;
                }

                tempGameObject = EditorSceneController.Instance.SpawnPrefab(prefab, position, false);
                tempSavable = EditorSceneController.Instance.AddRoad(tempGameObject, id);
            }
        }

        private void SaveRoad()
        {
            SavableRoad[] savables = EditorSceneController.Instance.Background.GetComponentsInChildren<SavableRoad>();
            selectedLevelRepresentation.roadsDataProperty.arraySize = savables.Length;
            SerializedProperty currentProperty;

            for (int i = 0; i < savables.Length; i++)
            {
                currentProperty = selectedLevelRepresentation.roadsDataProperty.GetArrayElementAtIndex(i);
                currentProperty.FindPropertyRelative(ID_PROPERTY_PATH).stringValue = savables[i].Id;
                currentProperty.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value = savables[i].Position;
            }
        }

        private void LoadFinish()
        {
            SerializedProperty currentProperty;
            string id;
            Vector3 position;
            GameObject tempGameObject;
            SavableFinish tempSavable;
            GameObject prefab;

            for (int i = 0; i < selectedLevelRepresentation.finishLevelDataProperty.arraySize; i++)
            {
                currentProperty = selectedLevelRepresentation.finishLevelDataProperty.GetArrayElementAtIndex(i);
                id = currentProperty.FindPropertyRelative(ID_PROPERTY_PATH).stringValue;
                position = currentProperty.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value;
                prefab = GetPrefab(ItemType.Finish, id);

                if (prefab == null)
                {
                    continue;
                }

                tempGameObject = EditorSceneController.Instance.SpawnPrefab(prefab, position);
                tempSavable = EditorSceneController.Instance.AddFinish(tempGameObject, id);
            }
        }

        private void SaveFinish()
        {
            SavableFinish[] savables = EditorSceneController.Instance.Content.GetComponentsInChildren<SavableFinish>();
            selectedLevelRepresentation.finishLevelDataProperty.arraySize = savables.Length;
            SerializedProperty currentProperty;

            for (int i = 0; i < savables.Length; i++)
            {
                currentProperty = selectedLevelRepresentation.finishLevelDataProperty.GetArrayElementAtIndex(i);
                currentProperty.FindPropertyRelative(ID_PROPERTY_PATH).stringValue = savables[i].Id;
                currentProperty.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value = savables[i].Position;
            }
        }

        private void LoadEnvironment()
        {
            SerializedProperty currentProperty;
            string id;
            Vector3 position;
            GameObject tempGameObject;
            SavableEnvironment tempSavable;
            GameObject prefab;

            for (int i = 0; i < selectedLevelRepresentation.environmentLevelDataProperty.arraySize; i++)
            {
                currentProperty = selectedLevelRepresentation.environmentLevelDataProperty.GetArrayElementAtIndex(i);
                id = currentProperty.FindPropertyRelative(ID_PROPERTY_PATH).stringValue;
                position = currentProperty.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value;
                prefab = GetPrefab(ItemType.Environment, id);

                if (prefab == null)
                {
                    continue;
                }

                tempGameObject = EditorSceneController.Instance.SpawnPrefab(GetPrefab(ItemType.Environment, id), position, false);
                tempSavable = EditorSceneController.Instance.AddEnvironment(tempGameObject, id);
            }
        }

        private void SaveEnvironment()
        {
            SavableEnvironment[] savables = EditorSceneController.Instance.Background.GetComponentsInChildren<SavableEnvironment>();
            selectedLevelRepresentation.environmentLevelDataProperty.arraySize = savables.Length;
            SerializedProperty currentProperty;

            for (int i = 0; i < savables.Length; i++)
            {
                currentProperty = selectedLevelRepresentation.environmentLevelDataProperty.GetArrayElementAtIndex(i);
                currentProperty.FindPropertyRelative(ID_PROPERTY_PATH).stringValue = savables[i].Id;
                currentProperty.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value = savables[i].Position;
            }
        }

        private GameObject GetPrefab(ItemType type, string id)
        {
            SerializedProperty property = GetItemTypeProperty(type);

            for (int i = 0; i < property.arraySize; i++)
            {
                if (property.GetArrayElementAtIndex(i).FindPropertyRelative(ID_PROPERTY_PATH).stringValue.Equals(id))
                {
                    return property.GetArrayElementAtIndex(i).FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue as GameObject;
                }
            }

            Debug.LogError($"Prefab not found id: {id} type: {property.displayName}");
            return null;
        }


        private bool IsNumerical(ItemType type, string id)
        {
            SerializedProperty property = GetItemTypeProperty(type);

            if (!((type == ItemType.Gates) || (type == ItemType.Boosters)))
            {
                Debug.LogError("IsNumerical called for wrong type");
                return false;
            }



            for (int i = 0; i < property.arraySize; i++)
            {
                if (property.GetArrayElementAtIndex(i).FindPropertyRelative(ID_PROPERTY_PATH).stringValue.Equals(id))
                {
                    return property.GetArrayElementAtIndex(i).FindPropertyRelative(IS_NUMERICAL_PROPERTY_PATH).boolValue;
                }
            }

            return false;
        }

        private SerializedProperty GetItemTypeProperty(ItemType type)
        {
            switch (type)
            {
                case ItemType.Gates:
                    return gatesDataProperty;
                case ItemType.Boosters:
                    return boostersDataProperty;
                case ItemType.Obstacles:
                    return obstaclesDataProperty;
                case ItemType.Enemies:
                    return enemiesDataProperty;
                case ItemType.Road:
                    return roadPrefabsProperty;
                case ItemType.Finish:
                    return finishPrefabsProperty;
                case ItemType.Environment:
                    return environmentPrefabsProperty;
                default:
                    Debug.LogError("Unknown type: " + type);
                    return null;
            }
        }


        private void SaveLevelItems()
        {
            SaveGates();
            SaveBoosters();
            SaveObstacles();
            SaveEnemies();
            SaveRoad();
            SaveFinish();
            SaveEnvironment();

            selectedLevelRepresentation.ApplyChanges();
            levelsHandler.UpdateCurrentLevelLabel(selectedLevelRepresentation.GetLevelLabel(levelsHandler.SelectedLevelIndex, stringBuilder));

            AssetDatabase.SaveAssets();
        }

        private bool SaveLevelIfPosssibleAndProceed(bool canUseCancel = true) //true == proceed 
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != EDITOR_SCENE_NAME)
            {
                return true;
            }

            if (selectedLevelRepresentation == null)
            {
                return true;
            }

            if (selectedLevelRepresentation.NullLevel)
            {
                return true;
            }

            if (EditorSceneController.Instance.IsLevelChanged())
            {
                if (canUseCancel)
                {
                    int optionIndex = EditorUtility.DisplayDialogComplex($"Level was modified", "Do you want to save the changes ?", "Save", "Cancel", "Don`t save");

                    if (optionIndex == 0) //save
                    {
                        SaveLevelItems();
                        levelsHandler.SetLevelLabels();
                        return true;
                    }
                    else if (optionIndex == 1) //Cancel
                    {
                        return false;
                    }
                    else // don`t save
                    {
                        return true;
                    }
                }
                else
                {
                    if (EditorUtility.DisplayDialog($"Level was modified", "Do you want to save the changes ?", "Save", "Don`t save"))
                    {
                        SaveLevelItems();
                    }

                    return true;
                }

            }
            else
            {
                selectedLevelRepresentation.ApplyChanges();
                levelsHandler.UpdateCurrentLevelLabel(selectedLevelRepresentation.GetLevelLabel(levelsHandler.SelectedLevelIndex, stringBuilder));
                AssetDatabase.SaveAssets();
            }

            return true;
        }

        private void OnDestroy()
        {
            SaveLevelIfPosssibleAndProceed(false);
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= BeforeAssemblyReload;

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                try
                {
                    EditorSceneController.Instance.Unsubscribe();
                }
                catch
                {

                }

                OpenScene(GAME_SCENE_PATH);
            }
        }

        [SerializeField]
        private enum ItemType
        {
            Gates = 0,
            Boosters = 1,
            Obstacles = 2,
            Enemies = 3,
            Finish = 4,
            Road = 5,
            Environment = 6
        };


        protected class LevelRepresentation : LevelRepresentationBase
        {
            private const string ROADS_DATA_PROPERTY = "roadsData";
            private const string FINISH_DATA_PROPERTY = "finishLevelData";
            private const string ENVIRONMENT_DATA_PROPERTY = "environmentLevelData";
            private const string PLAYER_SPAWN_POSITION_Z = "playerSpawnPositionZ";
            private const string CAMERA_DATA_ID_PROPERTY = "cameraDataId";
            private const string NOTE_PROPERTY = "note";
            public SerializedProperty gatesDataProperty;
            public SerializedProperty boostersDataProperty;
            public SerializedProperty obstaclesDataProperty;
            public SerializedProperty enemiesDataProperty;
            public SerializedProperty roadsDataProperty;
            public SerializedProperty finishLevelDataProperty;
            public SerializedProperty environmentLevelDataProperty;
            public SerializedProperty playerSpawnPositionZProperty;
            public SerializedProperty cameraDataIdProperty;
            public SerializedProperty noteProperty;


            //this empty constructor is nessesary
            public LevelRepresentation(UnityEngine.Object levelObject) : base(levelObject)
            {
            }


            protected override void ReadFields()
            {
                gatesDataProperty = serializedLevelObject.FindProperty(GATES_DATA_PROPERTY_NAME);
                boostersDataProperty = serializedLevelObject.FindProperty(BOOSTERS_DATA_PROPERTY_NAME);
                obstaclesDataProperty = serializedLevelObject.FindProperty(OBSTACLES_DATA_PROPERTY_NAME);
                enemiesDataProperty = serializedLevelObject.FindProperty(ENEMIES_DATA_PROPERTY_NAME);
                roadsDataProperty = serializedLevelObject.FindProperty(ROADS_DATA_PROPERTY);
                finishLevelDataProperty = serializedLevelObject.FindProperty(FINISH_DATA_PROPERTY);
                environmentLevelDataProperty = serializedLevelObject.FindProperty(ENVIRONMENT_DATA_PROPERTY);
                playerSpawnPositionZProperty = serializedLevelObject.FindProperty(PLAYER_SPAWN_POSITION_Z);
                cameraDataIdProperty = serializedLevelObject.FindProperty(CAMERA_DATA_ID_PROPERTY);
                noteProperty = serializedLevelObject.FindProperty(NOTE_PROPERTY);
            }

            public override void Clear()
            {
                if (!NullLevel)
                {
                    gatesDataProperty.arraySize = 0;
                    boostersDataProperty.arraySize = 0;
                    obstaclesDataProperty.arraySize = 0;
                    enemiesDataProperty.arraySize = 0;
                    roadsDataProperty.arraySize = 0;
                    finishLevelDataProperty.arraySize = 0;
                    environmentLevelDataProperty.arraySize = 0;
                    noteProperty.stringValue = string.Empty;
                    ApplyChanges();
                }

            }

            public override string GetLevelLabel(int index, StringBuilder stringBuilder)
            {
                stringBuilder.Clear();
                stringBuilder.Append(NUMBER);
                stringBuilder.Append(index + 1);
                stringBuilder.Append(SEPARATOR);

                if (NullLevel)
                {
                    stringBuilder.Append(NULL_FILE);
                }
                else
                {
                    if (noteProperty.stringValue.Length == 0)
                    {
                        noteProperty.stringValue = "Level " + (index + 1);
                        ApplyChanges();
                    }

                    stringBuilder.Append(noteProperty.stringValue);
                }

                return stringBuilder.ToString();
            }
        }
    }
}
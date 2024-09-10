# Courier
Continuing development fork of the Unity remake of Delivery Boy Task

## Setup
1. Build the Courier application.
1. Move the application to the "Desktop" folder.
1. Setup the config files:
    1. Create a folder named "data" in your "Desktop" folder.
    1. Copy the "configs" folder from the code base into the "data" folder.
    1. If your computer does not have a syncbox, open the experiment type you want to run and make sure the "noSyncbox" variable is set to "true".
    1. If you are running NICLServer on a different computer, make sure to change the "niclServerIP" in the "config.json" file.
1. Run the application:
    1. If asked to select a resolution, pick the closest one to 1920x1080.
    1. Fill in a valid participant ID.
    1. If you are using NICLServer, then make sure to tick the "Use Nicls" box (this needs to be renamed).
    1. Click Begin Session.

## Instructions for Audio + Prefab Files
First, please make sure to download the audio recordings from Box (or use your own, so long as your audio files have the same names as your prefabs) and upload them to Assets/Audio/StoreAudioEnglish/New Store. Also please make sure to download the prefab graphical asset packs and place them in the Assets/Retrieval Assets folder for things to work properly (there should be 5 folders of them in the Drive I've shared). Finally, put the "Object Prefab" folder in Assets/Models. Due to legal and technical reasons, users must download their own prefab files. Large prefab files are not efficient to store in Git and may not be possible/practical to include due to their file sizes. I've created some scripts to automate a lot of the work with integrating the prefabs into the GUI and generally making life easier. Feel free to follow these steps to work with your own prefab files:

1. **Download or Create Prefab Files**:
    - Ensure you have a folder containing your seelected prefabs. Place this folder within your Unity project in `Assets/Retrieval Assets/Object Prefabs'
    - Ensure you have a folder to source these prefabs' meshes and metadata (with matching GUIDs as those in Object Prefabs) with the path 'Assets/Retrieval Assets/Retrieval Assets'

2. **Using Provided Scripts**:
    - We provide three scripts in the `Editor` folder to help you manage and process your prefabs.

### FbxToPrefabConverter
This script converts a folder of `.fbx` files into `.prefab` files.

#### How to Use:
1. Open the Unity Editor.
2. Go to `Tools > Convert FBX to Prefabs`.
3. Set the `FBX Folder Path` to the folder containing your `.fbx` files.
4. Set the `Prefab Folder Path` to the folder where you want to save the generated prefabs.
5. Click `Convert`.

### ModelListAssigner
This script assigns a large chunk of prefabs to a serializable `ModelList` object.

#### How to Use:
1. Open the Unity Editor.
2. Go to `Tools > Assign Models to ModelList`.
3. Drag your `ModelList` ScriptableObject instance into the `Model List` field.
4. Set the `Prefabs Folder Path` to the folder containing your prefabs.
5. Click `Assign Models`.

### ShowObjectOnProximityAssigner
This script assigns the `ShowObjectOnProximity` component to prefabs and ensures they have necessary properties and components like our `ShowObjectOnProximity` script.

#### How to Use:
1. Open the Unity Editor.
2. Go to `Tools > Assign ShowObjectOnProximity`.
3. Set the `Prefabs Folder Path` to the folder containing your prefabs.
4. Click `Assign ShowObjectOnProximity`.

## Hints about editing this repo:
### How the experiment starts
The "MainMenu" scene is responsible for calling `DeliveryExperiment.ConfigureExperiment`, and then loading the MainGame scene. In the MainGame scene, just look at `DeliveryExperiment.Start` which launches the coroutine that controls the entire flow of the experiment.

### Steps to add a new store to DBoy:
First, create an object to represent the store:
1. Open the "MainGame" scene.
1. In the object hierarchy, look at the children of the "NamedStores" object.
1. Copy one (command c) and paste it (command v). For example, copy the toy_store object.
1. Rename your pasted object and make sure it is also a child of NamedStores. For example, rename it to toy_toy_store.
1. Move your object around in the scene view to place your new store. For example, move it in the negative X direction to position it next to the toy_store.
1. Change your object visually in some way. For example, make it smaller by reducing the "scale" values under the "transform" component.

Add your store to the master list of stores:
1. Under the "MainCoroutine" object's components, examine the DeliveryExperiment script. Expand the Environments list, then expand Element 0. Increase the length of the stores array by 1, and drag your new object into the new box. (Your new object should be a child of "NamedStores").

You will also need to create an object to display during the familiarization phase:
1. Under the "Faraway Parent" object, under "named stores," again copy one of the stores. For example, the toy store.
1. Again reparent the new object, rename it, and adjust its visual attributes if desired.
1. Click the "Faraway Parent" object, and look in the inspector at its "Familiarizer" component. Increase the size of the store array to 18, and add your new familiarization store.

Next, associate the store with audio items for the player to deliver:
1. Click the "NamedStores" object. Under the "store names to items" field, increase the size of the mapping to 18. (Add another store to map to items).
1. Expand the added entry in the mapping (most likely "toy store" at the bottom, a duplicate of the previous last entry). Give it a new name, for example, "toy toy store."
1. You will see a list of German and English audio files under your new entry. These can be reassigned by clicking and dragging new audio files into the boxes.

Make sure your store name has English and German translations:
1. In the LanguageSource script, edit language_string_dict. Add an entry with the name of your store object as the key, and an array of English and German names as the values. For example: `{ "toy toy store", new string[] {"toy toy store", "den Spielespielwarenladen"} }`.

If you run the game (remember to deselect "use ramulator"), there will be an additional store during the familiarization phase, and it will be in the world ready to have items delivered. It won't necessarily be named what you called it, since store names are randomized for each participant.

### Steps to increase the number of packages delivered each day by 1:
1. In the "DeliveryExperiment" script, edit the constant variable `DELIVERIES_PER_TRIAL`. Increase its value by 1.
1. (Note it's often useful to decrease this number to something low for testing purposes, if you want to get through delivery days quickly.)

### Steps to reverse the order of cued and free recall:
1. First, update your dboy to the latest version from GitHub if you haven't yet.
1. In the "DeliveryExperiment" script, locate the "subcoroutine" called `DoRecall()`.
1. Reverse the order of the calls to `DoFreeRecall` and `DoCuedRecall`.

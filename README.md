# Redback SmartBike SunCycle VR
The Unity project for the SmartBike SunCycle VR exercise system.

## Prerequisites
- [Unity3D](https://unity.com/releases/editor/archive#download-archive-2022) version 2022.3.22 installed
- IDE or editor of choice (e.g., MS Visual Studio, VS Code)
- Collaborator access to this repository (if you are a team member)
- Git credentials configured on your machine

### Recommended
- A VR headset, such as Meta Quest 2/3

### Optional
- Git GUI such as GitKraken, SourceTree, GitHub Desktop (if you prefer this over working only with terminal commands)

## Getting Started
1. Clone the repository to your local machine
    ```bash
    git clone https://github.com/Redback-Operations/redback-smartbike-vr.git
    ```
   (or SSH if you have set up SSH access to your GitHub account)
2. Create a branch for your work, following the Capstone [Branching Guidelines](https://verdant-raindrop-f3e404.netlify.app/processes/quality-assurance/git-contributions-guide/#branching-guidelines)
    ```bash
    git branch <branch-name>
    ```
3. Check out your branch
    ```bash
    git checkout <branch-name>
    ```
4. Open your terminal and navigate to the project directory
    ```bash
    cd path/to/redback-smartbike-vr
    ```
## Locations of Assets

The project has been setup to be clean and items will need to be placed in the correct locations. All assets will located under the Assets folder, with the differing types under their own folder. 

1. Animations - any animations that are linked to models in the scene, these are the Unity-based animations not the animations that are embedded inside imported FBX/OBJ
2. Audio - contain any music or sound effects
3. Materials - the materials that are either imported separately or created within the Unity project. The SunCycle project uses the Universal Render Pipeline, and if when importing an asset the material is colored pink change the material to use one of the URP-based materials
4. Models - and FBX/OBJ files that have been imported, created, generated into the Unity project - these are the geometry that is rendered in the virtual world
5. Prefabs - prefabs are objects that Unity uses to be able to implement in a scene easily - they will include items that have embedded scripts, objects, materials etc. to be reused in the project
6. Renderer - this holds the URP-based assets for the base configuration, if these are missing, assets will turn pink
7. Scenes - scenes could be described as levels within the Unity system, but can also be more - the main levels for the project are stored in this folder
8. Scripts - all C# based scripting is stored under this folder and if you need to configure scripts accessible within the Unity Editor, place them within the Scripts/Editor folder
9. Shaders - any extended shaders that are needed that the standed URP shaders cannot provide
10. Textures - the images that the materials utilize to place on the models, this folder is for images that are placed on 3D models
11. UI - the UI folder is used to store image assets that are used in the user interface rather than as 3D assets
12. XR / XRI - assets for the Unity XR system, this is currently implemented for use with Oculus XR but other systems can be added if you need to support the VR headset you own

When adding your new feature, add the files into the separate asset folders and, if necessary, create a sub-folder to store assets if there is great complexity.

## Useful links
- [Unity docs](https://docs.unity.com/)

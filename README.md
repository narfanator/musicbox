# musicbox

This project initially started as playing around with VR to make music. It quickly became apparent that a good user input library was necessary - so that's most of what this code it. Specifically, these scripts let you interact with the UnityUI system in VR.

## Features

- UnityUI system interaction from VR controllers (vive only atm)
- UnityUI system interaction from Editor game view
- Physics-object interaction from VR controllers
- Physics-object interaction from Editor game view (next planned feature)

In other words, you can click on VR buttons in the editor, and you can click on them using VR controllers; and you can do the same with objects in your scene.

## Usage

- Add one "VRMouseInput" to the EventSystem object to click on things in the editor game view
- Add one or more "VR3DInput" to the EventSystem object; add a sphere collider to your VR controllers, and add that sphere collider to the VR3DInput component, to interact with things via the controller (add one per controller)
- Add one of more "ViveControllerInput" to the EventSystem object; add a sphere collider to your Vive controller objects,  and add that sphere collider to this component, to interact with things via the controller (add one per controller)
- Add EventTrigger components to objects you want to interact with. Configure normally.

VRMouseInput will send pointer motion (enter, hover, exit), pointer click (down, held, up, click), drag (initializeDrag, beginDrag, drag, endDrag), and scroll (scroll) events.

VR3DInput will send pointer motion (enter, hover, exit) events. Write a script inheriting from this to handle your controller inputs; see the ViveControllerInput script for an examples. Objects are "under the pointer" when they are colliding with the sphere.

ViveControllerInput will send pointer motion, trigger click as pointer click, and trigger drag as pointer drag, events. Additional events are planned for grip, hair trigger, etc. Objects are "under the pointer" when they are colliding with the sphere.

Note that adding the Input modules will add a special module, "Multiple Input Modules Hack". This is to get around Unity's issue dealing.

## Development

VRInputModule is an abstract parent class to handle common functionality. Specifically, updating the list of entered, hovered and exited objects. Inherit from this if you want to make a new kind of cursor (other than the mouse in the editor pane, or a collider in the scene).

VR3DInputModule is a class to turn a sphere collider into a cursor. It will maintain lists of objects that interact with the cursor (entered, "hovered", exited). Inherit from this to handle a new kind of controller (say, Occulus controllers) - just make sure to call base.Process() at the start of your custom process function - then go on to handle button presses, etc.

ViveControllerInput is a class to handle the Vive controllers. Use it as an example - alter it if you want to change the button mappings, etc.

## Known Issues

- Dragging sliders around using the mouse is wonky, but works
- VR3DInput only detects UI elements that are in a line between the main camera, through the center of the sphere - it doesn't notice UI elements that are close enough but to the side.

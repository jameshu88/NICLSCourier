.. sectnum::

#############
Developer's Guide
#############
This is the overall guide to Courier setup and development

.. contents:: **Table of Contents**
    :depth: 2

*************
Overview
*************
<TODO>

*************
Important Code Concepts
*************
There are some very important structures within the code that are critical for all contributers to understand. They are listed below.
<TODO>

*************
Setup
*************
#. Clone the 'Courier Repo <https://github.com/pennmem/Courier>'_
#. Download 'Unity version 2021.3.2f1 <https://unity3d.com/get-unity/download/archive>'_. The two methods to do this are:
    #. Unity Hub (recommended)
    #. Unity Installer
#. Download the 'Textures folder <https://upenn.box.com/s/1s1ba0u6tf2hktigw0z7onxciduswpl1>'_ from *system_3_installers/delivery person/Town Constructor 3* in the UPenn Box
#. Put the newly downloaded Textures folder in the *Courier/Assets/Town Constructor 3* folder of the Courier Repo
#. Open the project in Unity
#. Go to *Assets > Scenes* in the project pane on the bottom of unity and double click the *MainGame* file to open the map
#. Download the 'Configs folder<https://upenn.box.com/s/r06s5e0f6zjd4sjyhb4l4ivjrucecwwb>'_.
#. Make a folder named "data" on your Desktop
#. Rename the configs folder to "configs" and put it in the new "data" folder

=============
Unity Hub Setup
=============
#. Download Unity Hub (https://docs.unity3d.com/Manual/GettingStartedInstallingHub.html)
#. Create Account
#. Click Activate New License Button (top left)
#. Select "Unity Plus or Pro" and put in Serial Number (ask James or Ryan for the serial number)
#. Click the "back arrow" in the top left to the top menu

..
#. Then follow the rest of the guide mentioned above (specifically the "Installing the Unity Editor" section)

Note: It does not show download or install percentage (just a bar), so you just have to wait patiently (edited)

*************
Load and Play the Game
*************
#. Go to *Assets > Scenes* in the project pane on the bottom of Unity and double click the *MainMenu* file
#. Click the play button at the top

*************
Reducing Motion Sickness
*************
This section describes common causes of motion sickness and how to minimize it.

Motion sickness itself is caused by multiple senses of the body not matching up in the way the mind expects them to (most often your visual system and your inner ear).

Note that this is distinct from how realistic the game is, but it is related. The more realistic a game seems, the better the senses need to match up (more complete apriori expectations in the mind).

=============
Acceleration
=============
Acceleration is the cause of almost all motion sickness. Accelerating forward and backwards are usually not much of a problem, but turning is!
The inner ear expects to feel all aceleration, but since the player is just sitting in a chair, they don't actually feel that. Ways to help are listed below

#. Reduce the speed of movement
#. Reduce the speed of movement only for turns (turns are the worst offenders of motion sickness)
#. Do not allow turning and forward movement at the same time (limits the amount of acceleration at any give time, but hard to make feel right)
#. Add large static items in the background (like a moon or a skyscraper) so that the person can fixate on the unchanging object. This only works for forward and backward acceleration
#. Adjust the field of view
    #. Reduce the field of view (FOV). Since we perceive acceleration as how fast things are moving past us, when you increase the FOV you can see more things whipping by. This makes it seem like you are going faster. This causes a larger feeling of motion sickness when turning. This is mostly only true when your actual FOV on the computer screen is significantly smaller than your actual FOV causing a "fish-eye effect". Just try to avoid that.
    #. Increase the field of view (FOV). This has been shown in studies and from personal experience to reduce motion sickness. This can be because it doesn't require the person to have to move the camera as much to see what they want to see. It can also make it more realistic to the person's vision depending on the setup.
    #. Probably stick to about 90deg FOV, but test for your monitor and placement so it seems realistic. At the end of the day, this changes from person to person anyway. Most games provide a FOV slider to help people, but with this being a psych experiment, we do not do that.

=============
Blurring
=============
#. Blurring has been shown to be a cause of increased motion sickness (https://pubmed.ncbi.nlm.nih.gov/25945660/). Do not blur unless you have a REALLY good reason to. 
#. That said, blurring during rotation could potentially decrease motion sickness (https://arxiv.org/abs/1710.02599). Note that the linked study is in VR.
#. Blurring can also occur due to the characteristics of the monitor. The GtG and MPRT are the most important specs of a monitor to reduce motion sickness. The article linked here is the best explanation that I've seen (https://blurbusters.com/gtg-versus-mprt-frequently-asked-questions-about-display-pixel-response/). This means that you have to reduce differences between objects that sit next to each other (like the edge of a wall and its background). When rotating fast (const quick acceleration) the wall edges will blur. This is especially potent with bright text on a dark wall.

=============
Player Input Does Not Match Character Movement (And Lag)
=============
This is a sneaky one that pops up when you're not expecting it.

#. An example of when this occurs is if you use a simple average of the joystick direction to set the new direction in an attempt to smooth a player's controls. Unfortunately, what this can actually do it cause a lag between the users input and their desired direction causing a feeling of sliding on ice. This lag can cause motion sickness.
#. Also, lag in general can be thought of as causing miniature accelerations and decelerations of the visual field as if a person is jolting their head very quickly. Have you ever watched an unstabilized GoPro video of someone on a bike? If not, try it: https://www.youtube.com/watch?v=HS5HVV5BPKQ. That video show how bad unstabilized and poorly stabilized video is (the poorly stabilized is similar to normal lag). This video shows what good stabilization is like: https://www.youtube.com/watch?v=ye-aOedNjQ8.

=============
Other Ideas to Consider
=============
#. Place a tiny dot or crosshair in the middle of the screen as a static fixation point (Dying Light 2 had this)
#. Radial blur to draw attention to a fixed point (this may not work)

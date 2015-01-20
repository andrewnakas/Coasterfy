# Coasterfy
**Procedurally generated rollercoasters with Motion Tracking from Project Tango devices.**


Coasterfy uses Project Tango motion tracking, C# hermite spline colliders and some simple scripting to procedurally generate a rollercoaster based on motion.

The intention is to automatically scale according to motion extent.

One will be able to easily draw and design a rollercoaster anywhere, or bring a Tango device to a real-world rollercoaster and capture it simply by riding in the front seat.

***It would be sweet if there was a platform to easily map, export and upload the world's rollercoasters to a database that would allow people to ride them all....***


Known issues:
* Freezes when trying to resume - go back to building
* Google Cardboard SDK is not working during the ride
* Occasional spline root glitches
* Limited spline root count


Where this will go in the short term:
* Spline scaling
* Speed scaling
* ADF to rollercoaster

The hermite spline controller is here:

http://wiki.unity3d.com/index.php/Hermite_Spline_Controller

The cart is from here:

https://sketchfab.com/models/hliAt7xc6YU2XjXh5G4lzmQwFmR and non-commercial.

Thanks to everyone developing the Tango and the creators of these amazing assets!

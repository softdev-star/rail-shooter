using UnityEngine;

//script file for handling interfaces, static classes, ect

//Interface for objects which can be hit by weapons fire
public interface iHurtable
{
    //Method will always communicate an amount of damage and the point of impact
    void Hit(int damage, Vector3 impact);
}


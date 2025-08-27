using UnityEngine;

public class CharacterVariableManager : MonoBehaviour
{
    public Variable<bool> isJumping = new Variable<bool>(false);
}

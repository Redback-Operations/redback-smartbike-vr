using System.Linq;
using UnityEngine;

public class RandomiseChild : MonoBehaviour
{
    private bool _randomised = false;

    void Start()
    {
        Randomise();
    }

    private void Randomise()
    {
        if (_randomised)
            return;

        _randomised = true;

        var children = transform.Cast<Transform>().ToList().OrderBy(t => Random.Range(0, 100)).ToList();

        children.ForEach(c => c.gameObject.SetActive(false));
        children[0].gameObject.SetActive(true);
    }
}

using System.Collections;
using UnityEngine;
using WarpWorld.CrowdControl;

public class DemoBidWarEffect : CCEffectBidWar
{
    // Use this for initialization
	void Start ()
    {
        StartCoroutine(SendRequests());
    }

    private IEnumerator PlaceBid(string name, uint bid)
    {
        bidFor = name;
        cost = bid;
        CrowdControl.instance.TestEffect(this);
        yield return new WaitForSeconds(2.0f);
    }

    private IEnumerator SendRequests()
    {
        yield return StartCoroutine(PlaceBid("Blue", 500));
        yield return StartCoroutine(PlaceBid("Red", 333));
        yield return StartCoroutine(PlaceBid("Green", 666));
        yield return StartCoroutine(PlaceBid("Red", 444));
        yield return StartCoroutine(PlaceBid("Red", 100));
        yield return StartCoroutine(PlaceBid("Green", 500));
        yield return StartCoroutine(PlaceBid("Blue", 500));
        yield return StartCoroutine(PlaceBid("Blue", 500));
        yield return StartCoroutine(PlaceBid("Blue", 500));
    }

    protected override EffectResult OnTriggerEffect(CCEffectInstance effectInstance)
    {
        return TriggerEffect(effectInstance, EffectResult.Success);
    }

    EffectResult TriggerEffect(CCEffectInstance effectInstance, EffectResult result)
    {
        Debug.LogFormat("[CC DEMO EFFECT]: New winner for {0}: {1}", displayName, effectInstance.parameters[0]);
        return result;
    }
}

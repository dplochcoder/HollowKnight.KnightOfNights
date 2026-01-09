using KnightOfNights.IC;
using KnightOfNights.Scripts.SharedLib;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

[Shim]
internal class RevekSongSuppressor : MonoBehaviour
{
    protected virtual void OnEnable() => RevekSongSummon.AddInterceptor(InterceptRevekSong);

    protected virtual void OnDisable() => RevekSongSummon.RemoveInterceptor(InterceptRevekSong);

    protected virtual bool InterceptRevekSong(List<FluteNote> song) => true;
}

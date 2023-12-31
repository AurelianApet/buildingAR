﻿/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using UnityEngine;

/// <summary>
/// Class control the map for the DF-GUI.
/// </summary>
[System.Serializable]
[AddComponentMenu("Infinity Code/Online Maps/Controls/DF-GUI Texture")]
public class OnlineMapsDFGUITextureControl : OnlineMapsControlBase2D
{
#if DFGUI
    private dfTextureSprite sprite;

    /// <summary>
    /// Singleton instance of OnlineMapsDFGUITextureControl control.
    /// </summary>
    public new static OnlineMapsDFGUITextureControl instance
    {
        get { return OnlineMapsControlBase.instance as OnlineMapsDFGUITextureControl; }
    }

    public override Vector2 GetCoords(Vector2 position)
    {
        Rect rect = GetRect();
        int countX = map.texture.width / OnlineMapsUtils.tileSize;
        int countY = map.texture.height / OnlineMapsUtils.tileSize;

        double px, py;
        map.GetPosition(out px, out py);
        map.projection.CoordinatesToTile(px, py, map.zoom, out px, out py);

        float rx = (rect.center.x - position.x) / rect.width * 2;
        float ry = (rect.center.y - position.y) / rect.height * 2;
        px -= countX / 2f * rx;
        py += countY / 2f * ry;

        map.projection.TileToCoordinates(px, py, map.zoom, out px, out py);
        return new Vector2((float)px, (float)py);
    }

    public override bool GetCoords(out double lng, out double lat, Vector2 position)
    {
        Rect rect = GetRect();
        int countX = map.texture.width / OnlineMapsUtils.tileSize;
        int countY = map.texture.height / OnlineMapsUtils.tileSize;

        double px, py;
        map.GetPosition(out px, out py);
        map.projection.CoordinatesToTile(px, py, map.zoom, out px, out py);

        float rx = (rect.center.x - position.x) / rect.width * 2;
        float ry = (rect.center.y - position.y) / rect.height * 2;
        px -= countX / 2f * rx;
        py += countY / 2f * ry;

        map.projection.TileToCoordinates(px, py, map.zoom, out lng, out lat);
        return true;
    }

	public override Rect GetRect()
    {
        return sprite.GetScreenRect();
    }

    protected override void OnEnableLate()
    {
        sprite = GetComponent<dfTextureSprite>();
        if (sprite == null)
        {
            Debug.LogError("Can not find dfTextureSprite.");
            OnlineMapsUtils.DestroyImmediate(this);
        }
    }

    public override void SetTexture(Texture2D texture)
    {
        base.SetTexture(texture);
        sprite.Texture = texture;
    }
#endif
}
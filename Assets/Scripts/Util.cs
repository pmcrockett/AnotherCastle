using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Util
{
	public static Vector3 FlattenVectorOnY(Vector3 _vector) {
		return new Vector3(_vector.x, 0, _vector.z).normalized;
	}
	public static bool IsGrounded(GameObject _obj, float _castRadius) {
		float colliderHeight = GetColliderHeight(_obj);
		RaycastHit fallCast;
		return Physics.SphereCast(_obj.transform.position, _castRadius, Vector3.down, out fallCast, 9999999, 0b10000000, QueryTriggerInteraction.Ignore)
			&& fallCast.distance <= colliderHeight / 2 + 0.01 - _castRadius;
	}

	public static float GetColliderHeight(GameObject _obj) {
		float colliderHeight;
		if (_obj.GetComponent<CapsuleCollider>()) {
			colliderHeight = _obj.GetComponent<CapsuleCollider>().height * _obj.transform.localScale.y;
			if (colliderHeight < _obj.GetComponent<CapsuleCollider>().radius * 2 * _obj.transform.localScale.x) colliderHeight = _obj.GetComponent<CapsuleCollider>().radius * 2 * _obj.transform.localScale.x;
			if (colliderHeight < _obj.GetComponent<CapsuleCollider>().radius * 2 * _obj.transform.localScale.z) colliderHeight = _obj.GetComponent<CapsuleCollider>().radius * 2 * _obj.transform.localScale.z;
		} else if (_obj.GetComponent<BoxCollider>()) {
			colliderHeight = _obj.GetComponent<BoxCollider>().size.y * _obj.transform.localScale.y;
		} else if (_obj.GetComponent<SphereCollider>()) {
			colliderHeight = _obj.GetComponent<SphereCollider>().radius * 2 * _obj.transform.localScale.y;
			if (colliderHeight < _obj.GetComponent<SphereCollider>().radius * 2 * _obj.transform.localScale.x) colliderHeight = _obj.GetComponent<CapsuleCollider>().radius * 2 * _obj.transform.localScale.x;
			if (colliderHeight < _obj.GetComponent<SphereCollider>().radius * 2 * _obj.transform.localScale.z) colliderHeight = _obj.GetComponent<CapsuleCollider>().radius * 2 * _obj.transform.localScale.z;
		} else if (_obj.GetComponent<MeshCollider>()) {
			colliderHeight = _obj.GetComponent<MeshCollider>().bounds.size.y * _obj.transform.localScale.y;
		} else return 0;
		return colliderHeight;
    }

	public static float GetColliderWidth(GameObject _obj) {
		float colliderWidth;
		if (_obj.GetComponent<CapsuleCollider>()) {
			colliderWidth = _obj.GetComponent<CapsuleCollider>().radius * 2;
		} else if (_obj.GetComponent<BoxCollider>()) {
			colliderWidth = _obj.GetComponent<BoxCollider>().size.x < _obj.GetComponent<BoxCollider>().size.z ? _obj.GetComponent<BoxCollider>().size.x : _obj.GetComponent<BoxCollider>().size.z;
		} else if (_obj.GetComponent<SphereCollider>()) {
			colliderWidth = _obj.GetComponent<SphereCollider>().radius * 2;
		} else if (_obj.GetComponent<MeshCollider>()) {
			colliderWidth = _obj.GetComponent<MeshCollider>().bounds.size.x < _obj.GetComponent<MeshCollider>().bounds.size.z ? _obj.GetComponent<MeshCollider>().bounds.size.x : _obj.GetComponent<MeshCollider>().bounds.size.z; ;
		} else return 0;
		colliderWidth *= _obj.transform.localScale.x < _obj.transform.localScale.z ? _obj.transform.localScale.x : _obj.transform.localScale.z;
		return colliderWidth;
	}

	public static GameObject CreateDialogBox(Canvas _canvas, List<string> _dialog, List<Camera> _cameras, float _width = 800, float _height = 400, float _leftRightPad = 24, float _topBottomPad = 24) {
		GameObject box = new GameObject();
		box.AddComponent<RectTransform>();
		box.AddComponent<CanvasRenderer>();
		box.AddComponent<Image>();
		GameObject textObj = new GameObject();
		TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
		DialogBox dialogBox = textObj.AddComponent<DialogBox>();

		box.transform.SetParent(_canvas.transform);
		box.GetComponent<Image>().color = new Color(0, 0, 0, 0.75f);
		box.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
		box.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
		box.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
		box.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
		box.GetComponent<RectTransform>().sizeDelta = new Vector2(_width, _height);
		box.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		box.name = "DialogBox";

		dialogBox.dialog = _dialog;
		dialogBox.cameras = _cameras;

		textObj.transform.SetParent(box.transform, false);
		text.rectTransform.anchoredPosition = Vector3.zero;
		text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
		text.rectTransform.anchorMin = new Vector2(0, 0);
		text.rectTransform.anchorMax = new Vector2(1, 1);
		text.rectTransform.offsetMax = new Vector2(_leftRightPad * -1, _topBottomPad * -1);
		text.rectTransform.offsetMin = new Vector2(_leftRightPad, _topBottomPad);
		textObj.name = "DialogText";

		dialogBox.DisplayNextDialog();

		return box;
    }

	/*	\’ – Output a Single quote
		\” – Output a double quote
		\ – Output a Backslash
		\n – Insert a newline
		\r – Insert a carriage-return
		\t – Insert a tab
		\0 – Insert a null character
		\b – Insert a backspace
	 */
	public static string FixNewline(string _str) {
		return _str.Replace("\\n", "\n");
    }
}
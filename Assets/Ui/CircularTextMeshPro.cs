using UnityEngine;
using TMPro;

/// <summary>
/// An extension of TextMeshPro that causes the text to be displayed in a
/// circular arc. Allows setting an initial starting angle.
///
/// Adapted from https://github.com/TonyViT/CurvedTextMeshPro and improved.
/// TonyViT's version has some unnecessary properties and doesn't use the
/// OnPreRenderText event, which allows for fewer mesh updates.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(TextMeshProUGUI))]
public class CircularTextMeshPro : MonoBehaviour {
    private TextMeshProUGUI m_TextComponent;

    [SerializeField] private float m_radius = 10.0f;

    [Tooltip("The radius of the text circle arc")]
    public float Radius {
        get => m_radius;
        set {
            m_radius = value;
            OnCurvePropertyChanged();
        }
    }

    [SerializeField]
    [Range(0f, 360f)]
    private float m_startAngle = 0f;

    [Tooltip("The initial starting angle of the text in the circle (in degrees).")]
    public float StartAngle {
        get => m_startAngle;
        set {
            m_startAngle = value;
            OnCurvePropertyChanged();
        }
    }

    private void Awake() {
        m_TextComponent = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable() {
        m_TextComponent.OnPreRenderText += UpdateTextCurve;
        OnCurvePropertyChanged();
    }

    private void OnDisable() {
        m_TextComponent.OnPreRenderText -= UpdateTextCurve;
    }

    protected void OnCurvePropertyChanged() {
        UpdateTextCurve(m_TextComponent.textInfo);
        m_TextComponent.ForceMeshUpdate();
    }

    protected void UpdateTextCurve(TMP_TextInfo textInfo) {
        Vector3[] vertices;
        Matrix4x4 matrix;

        // Iterate over each character in the text
        for (int i = 0; i < textInfo.characterInfo.Length; i++) {
            // Skip invisible characters
            if (!textInfo.characterInfo[i].isVisible)
                continue;

            // Get the index of the mesh and material for the current character
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

            // Get the vertices of the mesh for the current character
            vertices = textInfo.meshInfo[materialIndex].vertices;

            // Calculate the midpoint of the character's baseline
            Vector3 charMidBaselinePos = new Vector2(
                (vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2,
                textInfo.characterInfo[i].baseLine);

            // Adjust the vertices' positions relative to the character's midpoint
            for (int j = 0; j < 4; j++) {
                vertices[vertexIndex + j] += -charMidBaselinePos;
            }

            // Compute the transformation matrix for the current character
            matrix = ComputeTransformationMatrix(charMidBaselinePos, textInfo, i);

            // Apply the transformation matrix to each vertex of the character
            for (int j = 0; j < 4; j++) {
                vertices[vertexIndex + j] = matrix.MultiplyPoint3x4(vertices[vertexIndex + j]);
            }
        }
    }

    protected Matrix4x4 ComputeTransformationMatrix(Vector3 charMidBaselinePos, TMP_TextInfo textInfo, int charIdx) {
        // Calculate the radius for the current line
        float radiusForThisLine = m_radius + textInfo.lineInfo[textInfo.characterInfo[charIdx].lineNumber].baseline;

        // Calculate the circumference of the circle for the current line
        float circumference = 2 * radiusForThisLine * Mathf.PI;

        // Calculate the normalized position of the character along the circumference
        float normalizedPosition = charMidBaselinePos.x / circumference;

        // Calculate the angle in radians based on the character's position and the circumference,
        // incorporating the start angle.
        float angle = ((normalizedPosition - 0.5f) * 360 + 90 + m_startAngle) * Mathf.Deg2Rad;

        // Calculate the new x and y coordinates of the character's midpoint using sine and cosine
        float x0 = Mathf.Cos(angle);
        float y0 = Mathf.Sin(angle);

        // Create a new vector representing the new position of the character's midpoint
        Vector2 newMidBaselinePos = new Vector2(x0 * radiusForThisLine, -y0 * radiusForThisLine);

        // Calculate the rotation angle in degrees based on the character's new position
        float rotationAngle = -Mathf.Atan2(y0, x0) * Mathf.Rad2Deg - 90;

        // Create a transformation matrix using the new position, rotation, and scale
        return Matrix4x4.TRS(
            new Vector3(newMidBaselinePos.x, newMidBaselinePos.y, 0),
            Quaternion.AngleAxis(rotationAngle, Vector3.forward),
            Vector3.one
        );
    }
}
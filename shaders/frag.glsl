#version 330

in vec2 uvcoord;

uniform sampler2D tex;
uniform bool drawLineFlag;
uniform int objectId;
uniform int hoveredObject;
uniform int hoverEnabled;

out vec4 fragColor;

void main() {
  vec4 base = drawLineFlag ? vec4(1.0) : texture(tex, uvcoord);
  if (!drawLineFlag && hoverEnabled != 0 && hoveredObject >= 0 && objectId == hoveredObject) {
    base = mix(base, vec4(0.25, 0.75, 1.0, 1.0), 0.42);
  }
  fragColor = base;
}

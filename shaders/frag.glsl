#version 330

in vec2 uvcoord;

uniform sampler2D tex;
uniform bool drawLineFlag;

out vec4 fragColor;

void main() { fragColor = drawLineFlag ? vec4(1.0) : texture(tex, uvcoord); }

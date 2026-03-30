#version 330

in vec3 color;
out vec4 fragColor;

uniform bool drawLineFlag;

void main ()
{
    fragColor = vec4(drawLineFlag ? vec3(1.0) : color, 1.0);
}

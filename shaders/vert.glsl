#version 330

in vec3 position;
in vec2 uv;

out vec2 uvcoord;
uniform mat4 translation;
uniform mat4 view;
uniform mat4 projection;

void main() {
  uvcoord = uv;
  gl_Position = vec4(position, 1.0) * translation * view * projection;
}

#version 330

in vec3 position;
out vec3 color;
uniform mat4 transform;
uniform mat4 projection;

void main() {
  color = position + vec3(0.5);
  gl_Position = vec4(position, 1.0) * transform * projection;
}

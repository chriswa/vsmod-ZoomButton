using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace ZoomButton {

  /*
  REFERENCE:
  https://github.com/anegostudios/vsmodexamples/blob/master/Mods/ScreenOverlaySquintShader/src/ScreenOverlaySquintShader.cs
  https://gist.github.com/patriciogonzalezvivo/670c22f3966e662d2f83
  http://glslsandbox.com/e#71442.0
  https://www.geeks3d.com/20091020/shader-library-lens-circle-post-processing-effect-glsl/
  */

  public class SquintOverlayRenderer : IRenderer {
    MeshRef quadRef;
    ICoreClientAPI capi;
    public IShaderProgram overlayShaderProg;

    public float PercentZoomed = 0;

    public SquintOverlayRenderer(ICoreClientAPI capi) {
      this.capi = capi;
      MeshData quadMesh = QuadMeshUtil.GetCustomQuadModelData(-1, -1, 0, 2, 2);
      quadMesh.Rgba = null;
      quadRef = capi.Render.UploadMesh(quadMesh);

      LoadShader();
      capi.Event.ReloadShader += LoadShader;
      capi.Event.RegisterRenderer(this, EnumRenderStage.Ortho);
    }

    public double RenderOrder => 1.1;

    public int RenderRange => 1;

    public void Dispose() {
      capi.Render.DeleteMesh(quadRef);
      overlayShaderProg.Dispose();
    }

    public void OnRenderFrame(float deltaTime, EnumRenderStage stage) {
      if (PercentZoomed == 0) return;

      IShaderProgram curShader = capi.Render.CurrentActiveShader;
      curShader.Stop();
      overlayShaderProg.Use();
      capi.Render.GlToggleBlend(true);
      overlayShaderProg.Uniform("percentZoomed", PercentZoomed);
      capi.Render.RenderMesh(quadRef);
      overlayShaderProg.Stop();
      curShader.Use();
    }

    public bool LoadShader() {
      overlayShaderProg = capi.Shader.NewShaderProgram();
      overlayShaderProg.VertexShader = capi.Shader.NewShader(EnumShaderType.VertexShader);
      overlayShaderProg.FragmentShader = capi.Shader.NewShader(EnumShaderType.FragmentShader);

      overlayShaderProg.VertexShader.Code = GetVertexShaderCode();
      overlayShaderProg.FragmentShader.Code = GetFragmentShaderCode();

      capi.Shader.RegisterMemoryShaderProgram("squintoverlay", overlayShaderProg);
      overlayShaderProg.Compile();

      return true;
    }

    private string GetVertexShaderCode() {
      return @"
        #version 330 core
        #extension GL_ARB_explicit_attrib_location: enable

        #ifdef GL_ES
        precision mediump float;
        #endif

        #extension GL_OES_standard_derivatives : enable

        layout(location = 0) in vec3 vertex;

        out vec2 uv;

        void main(void) {
          gl_Position = vec4(vertex.xy, 0, 1);
          uv = (vertex.xy + 1.0) / 2.0;
        }
      ";
    }

    private string GetFragmentShaderCode() {
      return @"
        #version 330 core

        in vec2 uv;
        out vec4 outColor;

        uniform float percentZoomed;
        uniform vec2 resolution;

        void main () {
          float dist = distance(uv.xy, vec2(0.5,0.5));
          float viewStrength = smoothstep(0.45, 0.38, dist * smoothstep(-1, 1, percentZoomed));
          outColor = vec4(0, 0, 0, min(0.8, 1 - viewStrength));
        }
      ";
    }
  }
}

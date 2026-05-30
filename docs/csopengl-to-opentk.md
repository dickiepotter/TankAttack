# From CSOpenGL to OpenTK

Tank Attack (2004) was written in C# against **CSOpenGL** — a binding the author's
companion project describes as "incomplete and un-maintained" — and ran inside a
**WinForms** host, decoding textures with `System.Drawing.Bitmap`. None of that
loads on a modern machine. This port moves the game to **OpenTK 4 on .NET 8**,
keeping the original fixed-function, immediate-mode rendering intact and replacing
only the dead plumbing.

## API name mapping

| Concept | CSOpenGL (2004) | OpenTK 4 |
| --- | --- | --- |
| GL calls | `GL.glBegin(GL.GL_QUADS)` | `GL.Begin(PrimitiveType.Quads)` |
| Constants | loose `int` | typed enums |
| Texture coords / verts | `GL.glTexCoord2d` / `glVertex3d` | `GL.TexCoord2` / `GL.Vertex3` |
| GLU view | `GLU.gluPerspective` / `gluLookAt` | `Core/Glu.cs` |
| GLU quadrics | `GLU.gluSphere` / `gluCylinder` (+ a `GLUquadric`) | `Core/Quadric.cs` |
| Texture decode | `System.Drawing.Bitmap` | `StbImageSharp` |
| Window / surface | `GLSurface` (a WinForms control) | `GameWindow` |
| Init / render | `surface.Init` / `surface.Render` events | `OnLoad` / `OnRenderFrame` |
| Input | WinForms `KeyDown` | per-frame `KeyboardState` polling + `OnKeyDown` |

## What had to be re-implemented

- **GLU quadrics (`Core/Quadric.cs`).** OpenTK has no GLU, so `gluSphere` (the
  shell) and `gluCylinder` (the gun barrel) are drawn in immediate mode with
  outward normals and optional texture coordinates, matching
  `gluQuadricNormals(SMOOTH)` / `gluQuadricTexture`. The `GLUquadric` handle
  becomes a small `Quadric` object.
- **GLU view (`Core/Glu.cs`).** `gluPerspective` (via `glFrustum`) and `gluLookAt`
  (a constructed view matrix), as the classic GLU did.
- **Texture loading (`TextureCollection`).** `System.Drawing.Bitmap` is Windows-
  only and, as used here, fed 24-bpp **BGR** pixels to GL as `GL_RGB`. StbImageSharp
  decodes true RGB cross-platform — which also **fixes a latent red/blue swap** the
  original shipped with.

## Default-state differences worth knowing

- **Context profile (the #1 gotcha).** Immediate mode, the matrix stack, fog,
  `GL_QUADS` and fixed-function lighting were all removed from core OpenGL 3.1+.
  OpenTK defaults to a *Core* profile that rejects them, so `TankWindow` requests
  `ContextProfile.Compatability`.
- **Lighting is configured but off by default.** `GLInit` sets up `LIGHT0`/`LIGHT1`
  and `glColorMaterial`, but never enables `GL_LIGHTING` — the world is drawn with
  full-bright textures (vertex colour white). The `F1`/`F2`/`F3` toggles turn
  lighting/fog on. This default-off behaviour is preserved exactly; enabling
  lighting "to be helpful" would have darkened the whole scene.
- **Texture orientation.** `System.Drawing.Bitmap` and StbImageSharp both deliver
  rows top-first, and `glTexImage2D` treats the first row as the texture's bottom —
  so both put the image the same way up. The hand-authored texture coordinates
  therefore line up without any vertical flip (unlike a "correctly" flipped loader).
- **Frame pacing.** The WinForms version redrew off the surface's own loop; the
  OpenTK loop is capped at 60 fps so per-frame movement runs at a sane speed.

## Intentional fixes (documented in code)

- The **explosion** scaled by `counter / 100` using *integer* division, so it was
  invisible until the counter passed 100 then jumped to full size. Now uses float
  division so it grows smoothly.
- **Aspect ratio** was hard-coded to `1` despite a wide window; it now tracks the
  main viewport so the view isn't stretched. (The minimap viewport is left exactly
  as the original authored it, clipping and all.)

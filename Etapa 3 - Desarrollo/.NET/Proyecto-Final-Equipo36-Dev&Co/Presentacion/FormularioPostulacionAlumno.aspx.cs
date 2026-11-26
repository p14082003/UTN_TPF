using Dominio;
using Negocio;
using Presentacion.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class FormularioPostulacionAlumno : System.Web.UI.Page
    {
        private UsuarioSesion usuario;
        private int idInstitucion;
        private string nombreInstitucion;
        private string ClaveCursos => "cursosPostulacion:" + idInstitucion;

        protected string CursosJson
        {
            get
            {
                var cursos = Session[ClaveCursos] as List<CursoApi> ?? new List<CursoApi>();
                var niveles = cursos.GroupBy(c => new { c.IdNivel, c.NivelPersonalizado })
                    .Select(grupo => new { Valor = grupo.First().IdCurso, Cursos = grupo.ToList() });
                return new JavaScriptSerializer().Serialize(niveles);
            }
        }

        // Documentos que hay que adjuntar para poder enviar la postulación (coinciden con el Value del desplegable)
        private static readonly string[] Obligatorios = { "DNI del alumno - Frente", "DNI del alumno - Dorso", "Partida de nacimiento" };
        private static readonly string[] DocumentosUnicos = { "DNI del alumno - Frente", "DNI del alumno - Dorso", "Partida de nacimiento", "Certificado de estudios" };

        private string ClaveDocumentos
        {
            get
            {
                if (ViewState["CargaDocumentos"] == null)
                {
                    ViewState["CargaDocumentos"] = Guid.NewGuid().ToString("N");
                }
                return "documentosPostulacion:" + ViewState["CargaDocumentos"];
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Form.Enctype = "multipart/form-data";
            usuario = UsuarioSesion.Obtener(Session);

            // Validar que el usuario sea un responsable
            if (usuario == null || usuario.IdTipoUsuario != UsuarioSesion.Responsable || usuario.IdResponsable.GetValueOrDefault() <= 0)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            Response.Cache.SetNoStore(); // Evita que la pagina guarde cache

            // Valido que el formulario reciba el idInstitucion de la seleccionada en el detalle de la institucion
            if (!int.TryParse(Request.QueryString["id"], out idInstitucion) || idInstitucion <= 0)
            {
                Response.Redirect("~/ListadoInstituciones.aspx");
                return;
            }

            nombreInstitucion = Request.QueryString["nombre"];

            if(string.IsNullOrWhiteSpace(nombreInstitucion))
            {
                Response.Redirect("~/ListadoInstituciones.aspx");
                return;
            }

            txtInstitucion.Text = nombreInstitucion;

            if (!IsPostBack)
            {
                // Traer los cursos al cargar por primera vez el formulario
                RegisterAsyncTask(new PageAsyncTask(CargarFormularioAsync));
            }
        }

        private async Task CargarFormularioAsync()
        {
            await CargarCursosAsync(idInstitucion);
            try
            {
                var alumnos = await InstiRedApi.GetAsync<List<AlumnoApi>>(
                    "vw/ResponsablesConAlumnos?idResponsable=" + usuario.IdResponsable.Value);
                CargarAlumnos(alumnos.Where(a => a.IdAlumno.HasValue)
                    .GroupBy(a => a.IdAlumno).Select(g => g.First()).ToList());
                if (ddlAlumnos.Items.Count == 1)
                {
                    btnGuardar.Enabled = false;
                    MostrarMensaje("Todavía no tenés alumnos registrados. Registrá uno desde la opción “Registrar alumno” del menú.", false);
                }
            }
            catch (Exception ex)
            {
                Trace.Warn("FormularioPostulacionAlumno", "Error al consultar los alumnos del responsable", ex);
                btnGuardar.Enabled = false;
                MostrarMensaje("No pudimos cargar tus alumnos. Volvé a abrir el formulario para intentar nuevamente.", false);
            }
        }

        private void CargarAlumnos(List<AlumnoApi> alumnos)
        {
            ddlAlumnos.Items.Clear();
            ddlAlumnos.Items.Add(new ListItem("Seleccionar alumno", ""));

            if (alumnos == null)
            {
                return;
            }

            foreach (AlumnoApi alumno in alumnos)
            {
                string nombreCompleto = alumno.NombreAlumno + " " + alumno.ApellidoAlumno;

                ddlAlumnos.Items.Add(new ListItem(nombreCompleto, alumno.IdAlumno.ToString()));
            }
        }

        private async Task CargarCursosAsync(int idInstitucion)
        {
            try
            {
                // "await" permite esperar hasta que la API (El endpoint) devuelva un resultado
                List<CursoApi> cursos = await InstiRedApi.GetAsync<List<CursoApi>>("vw/Cursos?idInstitucion=" + idInstitucion);

                Session[ClaveCursos] = cursos;

                CargarCiclosLectivos(cursos);
                CargarNivelesEducativos(cursos);
                CargarAnios(cursos);
                CargarDivisiones(cursos);
                CargarTurnos(cursos);
                if (cursos.Count == 0)
                {
                    btnGuardar.Enabled = false;
                    MostrarMensaje("Esta institución todavía no tiene cursos publicados.", false);
                }
            }
            catch(Exception ex)
            {
                // Trace.Warn es una forma de .NET de almacenar errores
                Trace.Warn("FormularioPostulacionAlumno", "Error al consultar los cursos de la institucion", ex);
                Response.StatusCode = 502;
                Response.TrySkipIisCustomErrors = true;
                btnGuardar.Enabled = false;
                MostrarMensaje("No pudimos cargar los cursos. Volvé a abrir el formulario para intentar nuevamente.", false);
            }
        }

        private void CargarCiclosLectivos(List<CursoApi> cursos)
        {
            ddlCiclo.Items.Clear();

            ddlCiclo.Items.Add(new ListItem("Seleccionar ciclo", ""));

            // Ciclos lectivos de la institucion ordenados y sin repetir
            List<int> ciclos = cursos.Select(c => c.CicloLectivo).Distinct().OrderByDescending(c => c).ToList();

            // Agregar cada ciclo lectivo disponible al ddl
            foreach(int ciclo in ciclos)
            {
                ddlCiclo.Items.Add(new ListItem(ciclo.ToString(), ciclo.ToString()));
            }
        }

        private void CargarNivelesEducativos(List<CursoApi> cursos)
        {
            ddlNivel.Items.Clear();

            ddlNivel.Items.Add(new ListItem("Seleccionar nivel", ""));

            // Se registran todas las opciones; el navegador muestra las del ciclo elegido.
            List<CursoApi> niveles = cursos.GroupBy(c => new { c.IdNivel, c.NivelPersonalizado }).Select(g => g.First()).ToList();

            foreach (CursoApi curso in niveles)
            {
                string descripcion = curso.Nivel;

                if (string.IsNullOrWhiteSpace(descripcion))
                {
                    descripcion = curso.NivelPersonalizado;
                }
                else if (!string.IsNullOrWhiteSpace(curso.NivelPersonalizado) && curso.NivelPersonalizado != descripcion)
                {
                    descripcion += " - " + curso.NivelPersonalizado;
                }

                ddlNivel.Items.Add(new ListItem(descripcion, curso.IdCurso.ToString()));
            }

            // NOTA: Por como esta armada la DB, y los registros insertados, los cursos pueden tener nivel normal y
            //  el nivel personalizado al mismo tiempo. Por lo tanto hay que contemplar el caso en el que esten ambos
        }

        private void CargarAnios(List<CursoApi> cursos)
        {
            ddlAnio.Items.Clear();

            ddlAnio.Items.Add(new ListItem("Seleccionar año", ""));

            List<CursoApi> anios = cursos.GroupBy(c => c.Anio).Select(g => g.First()).OrderBy(c => c.Anio).ToList();

            foreach (CursoApi curso in anios)
            {
                string descripcion = curso.Anio.HasValue ? curso.Anio.Value.ToString() : "Sin año";

                string valor = curso.Anio.HasValue? curso.Anio.Value.ToString() : "null";

                ddlAnio.Items.Add(new ListItem(descripcion, valor));
            }
        }

        private void CargarDivisiones(List<CursoApi> cursos)
        {
            ddlDivision.Items.Clear();

            ddlDivision.Items.Add(new ListItem("Seleccionar división", ""));

            List<CursoApi> divisiones = cursos.GroupBy(c => c.Division).Select(g => g.First()).OrderBy(c => c.Division).ToList();

            foreach (CursoApi curso in divisiones)
            {
                ddlDivision.Items.Add(new ListItem(curso.Division, curso.Division));
            }
        }

        private void CargarTurnos(List<CursoApi> cursos)
        {
            ddlTurno.Items.Clear();

            ddlTurno.Items.Add(new ListItem("Seleccionar turno", ""));

            List<CursoApi> turnos = cursos.GroupBy(c => c.IdTurno).Select(g => g.First()).OrderBy(c => c.Turno).ToList();

            foreach (CursoApi curso in turnos)
            {
                ddlTurno.Items.Add(new ListItem(curso.Turno, curso.IdTurno.ToString()));
            }
        }

        private CursoApi ObtenerCursoSeleccionado(List<CursoApi> cursos)
        {
            int ciclo;
            int idNivel;
            int idTurno;
            if (cursos == null || !int.TryParse(ddlCiclo.SelectedValue, out ciclo) ||
                !int.TryParse(ddlNivel.SelectedValue, out idNivel) ||
                !int.TryParse(ddlTurno.SelectedValue, out idTurno) ||
                string.IsNullOrEmpty(ddlAnio.SelectedValue) || string.IsNullOrEmpty(ddlDivision.SelectedValue))
            {
                return null;
            }

            int anio;
            int? anioSeleccionado = null;
            if (ddlAnio.SelectedValue != "null")
            {
                if (!int.TryParse(ddlAnio.SelectedValue, out anio)) return null;
                anioSeleccionado = anio;
            }
            var nivel = cursos.FirstOrDefault(c => c.IdCurso == idNivel);
            if (nivel == null) return null;

            return cursos.FirstOrDefault(c => c.IdInstitucion == idInstitucion && c.CicloLectivo == ciclo &&
                c.IdNivel == nivel.IdNivel && c.NivelPersonalizado == nivel.NivelPersonalizado &&
                c.Anio == anioSeleccionado && c.Division == ddlDivision.SelectedValue && c.IdTurno == idTurno);
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!pnlFormulario.Visible)
            {
                return;
            }
            var documentos = ObtenerDocumentos();
            rptDocumentos.DataSource = documentos;
            rptDocumentos.DataBind();
            MostrarFaltantes(documentos);
            ActualizarTiposDocumento(documentos);
        }

        private void ActualizarTiposDocumento(List<DocumentoAdjunto> documentos)
        {
            // Enabled=false hace que WebForms omita la opción; disabled la mantiene visible y grisada.
            foreach (ListItem tipo in ddlTipoDocumento.Items)
            {
                tipo.Enabled = true;
                bool cargado = DocumentosUnicos.Contains(tipo.Value) && documentos.Any(d => d.Tipo == tipo.Value);
                if (cargado) tipo.Attributes["disabled"] = "disabled";
                else tipo.Attributes.Remove("disabled");
            }
            if (ddlTipoDocumento.SelectedItem == null || ddlTipoDocumento.SelectedItem.Attributes["disabled"] != null)
            {
                ddlTipoDocumento.ClearSelection();
                var siguiente = ddlTipoDocumento.Items.Cast<ListItem>().FirstOrDefault(tipo => tipo.Attributes["disabled"] == null);
                if (siguiente != null) siguiente.Selected = true;
            }
        }

        private List<string> ObtenerFaltantes(List<DocumentoAdjunto> documentos)
        {
            return Obligatorios.Where(o => !documentos.Any(d => d.Tipo == o)).ToList();
        }

        private void MostrarFaltantes(List<DocumentoAdjunto> documentos)
        {
            List<string> faltantes = ObtenerFaltantes(documentos);
            if (faltantes.Count == 0)
            {
                lblFaltantes.Text = "Ya adjuntaste los documentos obligatorios.";
                lblFaltantes.CssClass = "d-block mb-3 fg-success";
            }
            else
            {
                lblFaltantes.Text = "Falta adjuntar: " + string.Join(", ", faltantes) + ".";
                lblFaltantes.CssClass = "d-block mb-3 text-indigo";
            }
        }

        private List<DocumentoAdjunto> ObtenerDocumentos()
        {
            var documentos = Session[ClaveDocumentos] as List<DocumentoAdjunto>;
            if (documentos == null)
            {
                documentos = new List<DocumentoAdjunto>();
                Session[ClaveDocumentos] = documentos;
            }
            return documentos;
        }

        private void AgregarDocumentos()
        {
            var documentos = ObtenerDocumentos();
            var archivos = fuDocumentos.PostedFiles.Where(archivo => !string.IsNullOrEmpty(archivo.FileName)).ToList();
            if (archivos.Count == 0) return;

            string tipo = ddlTipoDocumento.SelectedValue;
            if (ddlTipoDocumento.Items.FindByValue(tipo) == null)
            {
                throw new ArgumentException("Seleccioná un tipo de documento válido.");
            }
            if (DocumentosUnicos.Contains(tipo))
            {
                if (documentos.Any(d => d.Tipo == tipo))
                {
                    throw new ArgumentException("Ya adjuntaste " + tipo + ". Quitá el archivo anterior si querés reemplazarlo.");
                }
                if (archivos.Count > 1)
                {
                    throw new ArgumentException("Solo podés adjuntar un archivo de tipo " + tipo + ".");
                }
            }
            var nuevos = new List<DocumentoAdjunto>();
            long total = documentos.Sum(d => (long)d.Contenido.Length);

            foreach (HttpPostedFile archivo in archivos)
            {
                if (archivo.ContentLength <= 0 || archivo.ContentLength > 5 * 1024 * 1024)
                {
                    throw new ArgumentException("Cada documento debe tener contenido y pesar como máximo 5 MB.");
                }
                string nombre = Path.GetFileName(archivo.FileName);
                string extension = Path.GetExtension(nombre).ToLowerInvariant();
                if (extension != ".pdf" && extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                {
                    throw new ArgumentException("Solo se permiten documentos PDF, JPG o PNG.");
                }
                total += archivo.ContentLength;
                if (total > 19 * 1024 * 1024)
                {
                    throw new ArgumentException("Los documentos de una postulación no pueden superar los 19 MB en total.");
                }

                // Los documentos libres conservan el nombre sin confundirse con los tipos obligatorios.
                string descripcion = tipo;
                if (tipo == "Otro documento")
                {
                    string nombreDocumento = Path.GetFileNameWithoutExtension(nombre).Trim();
                    descripcion = "Otro documento: " + (nombreDocumento.Length == 0 ? "Documento" : nombreDocumento);
                }
                if (descripcion.Length > 100)
                {
                    descripcion = descripcion.Substring(0, 100);
                }
                using (var copia = new MemoryStream())
                {
                    archivo.InputStream.CopyTo(copia);
                    nuevos.Add(new DocumentoAdjunto { Nombre = nombre, Tipo = tipo, Descripcion = descripcion, Contenido = copia.ToArray() });
                }
            }
            documentos.AddRange(nuevos);
        }

        protected void btnSubirDocumentos_Click(object sender, EventArgs e)
        {
            try
            {
                if (!fuDocumentos.HasFiles)
                {
                    throw new ArgumentException("Seleccioná al menos un documento para adjuntar.");
                }
                AgregarDocumentos();
                lblMensaje.Visible = false;
            }
            catch (ArgumentException ex)
            {
                MostrarMensaje(ex.Message, false);
            }
        }

        protected void rptDocumentos_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Eliminar")
            {
                var documentos = ObtenerDocumentos();
                int indice;
                if (int.TryParse(e.CommandArgument.ToString(), out indice) && indice >= 0 && indice < documentos.Count)
                {
                    documentos.RemoveAt(indice);
                }
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (btnGuardar.Enabled)
            {
                RegisterAsyncTask(new PageAsyncTask(EnviarPostulacionAsync));
            }
        }

        private async Task EnviarPostulacionAsync()
        {
            try
            {
                int idAlumno;
                var cursos = Session[ClaveCursos] as List<CursoApi>;
                var curso = ObtenerCursoSeleccionado(cursos);
                if (curso == null)
                {
                    throw new ArgumentException("Seleccioná el curso completo: ciclo, nivel, año, división y turno. Si la sesión venció, volvé a abrir el formulario.");
                }
                int idCurso = curso.IdCurso;
                if (!int.TryParse(ddlAlumnos.SelectedValue, out idAlumno) || idAlumno <= 0)
                {
                    throw new ArgumentException("Seleccioná el alumno que querés postular.");
                }

                AgregarDocumentos();
                var documentos = ObtenerDocumentos();
                List<string> faltantes = ObtenerFaltantes(documentos);
                if (faltantes.Count > 0)
                {
                    throw new ArgumentException("Faltan documentos obligatorios: " + string.Join(", ", faltantes) + ".");
                }

                using (var formulario = new MultipartFormDataContent())
                {
                    formulario.Add(new StringContent(usuario.IdResponsable.Value.ToString()), "IdResponsable");
                    formulario.Add(new StringContent(idAlumno.ToString()), "IdAlumno");
                    formulario.Add(new StringContent(idCurso.ToString()), "IdCurso");
                    for (int indice = 0; indice < documentos.Count; indice++)
                    {
                        formulario.Add(new StringContent(documentos[indice].Descripcion), "Documentos[" + indice + "].Descripcion");
                        formulario.Add(new ByteArrayContent(documentos[indice].Contenido),
                            "Documentos[" + indice + "].Archivo", documentos[indice].Nombre);
                    }

                    var respuesta = await InstiRedApi.PostAsync("sp/RegistrarPostulacionAlumno", formulario, leerCuerpo: true);
                    if (respuesta.Estado == HttpStatusCode.Created)
                    {
                        btnGuardar.Enabled = false;
                        pnlFormulario.Visible = false;
                        Session.Remove(ClaveDocumentos);
                        MostrarMensaje("La postulación quedó pendiente de revisión por la institución.", true);
                        var registro = new JavaScriptSerializer().Deserialize<ResultadoPostulacion>(respuesta.Cuerpo);
                        if (registro != null && !string.IsNullOrWhiteSpace(registro.CodigoSeguimiento))
                        {
                            MostrarMensaje("La postulación quedó pendiente de revisión por la institución. Guardá el código de seguimiento: " + registro.CodigoSeguimiento + ".", true);

                            //Enviar email
                            EnviarCorreo(usuario.Email, usuario.Nombre, usuario.Apellido, ddlAlumnos.SelectedItem.Text, nombreInstitucion, cursos.FirstOrDefault(c => c.IdCurso == idCurso && c.IdInstitucion == idInstitucion), registro.CodigoSeguimiento);
                        }
                        return;
                    }
                    if (respuesta.Estado == HttpStatusCode.BadRequest)
                    {
                        MostrarMensaje(respuesta.Mensaje, false);
                        return;
                    }
                    if (respuesta.Estado == HttpStatusCode.RequestEntityTooLarge)
                    {
                        MostrarMensaje("La carga supera el tamaño permitido. Quitá algún documento e intentá nuevamente.", false);
                        return;
                    }
                }
                MostrarMensaje("No se pudo completar la postulación. Intentá nuevamente más tarde.", false);
            }
            catch (ArgumentException ex)
            {
                MostrarMensaje(ex.Message, false);
            }
            catch (Exception ex)
            {
                Trace.Warn("FormularioPostulacionAlumno", "Error al enviar la postulación", ex);
                MostrarMensaje("No pudimos confirmar el resultado de la postulación. Consultá su estado antes de volver a enviarla.", false);
            }
        }

        private void MostrarMensaje(string mensaje, bool exito)
        {
            lblMensaje.Visible = true;
            lblMensaje.CssClass = exito ? "alert alert-success d-block m-4" : "alert alert-danger d-block m-4";
            lblMensaje.Text = HttpUtility.HtmlEncode(mensaje);
        }

        private void EnviarCorreo(string email, string nombreResponsable, string apellidoResponsable, string alumno, string institucion, CursoApi curso, string codigoSeguimiento)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                Trace.Warn("FormularioPostulacionAlumno", "No se pudo enviar el correo: el responsable no tiene un email registrado.");
                return;
            }

            if (string.IsNullOrWhiteSpace(nombreResponsable) || string.IsNullOrWhiteSpace(apellidoResponsable) || string.IsNullOrWhiteSpace(alumno) || string.IsNullOrWhiteSpace(institucion) || string.IsNullOrWhiteSpace(codigoSeguimiento))
            {
                Trace.Warn("FormularioPostulacionAlumno", "No se pudo enviar el correo: faltan datos de la postulación.");
                return;
            }

            if (curso == null || curso.IdCurso <= 0)
            {
                Trace.Warn("FormularioPostulacionAlumno", "No se puede enviar el correo: El id de curso no es válido");
                return;
            }

            string asunto = "Postulación de Alumno Registrada - InstiRed";
            string cuerpo = CuerpoPostulacionAlumno(nombreResponsable, apellidoResponsable, alumno, institucion, curso, codigoSeguimiento);

            try
            {
                EmailService emailService = new EmailService();
                emailService.armarCorreo(email, asunto, cuerpo);
                emailService.enviarEmail();
            }
            catch (Exception ex)
            {
                Trace.Warn("FormularioPostulacionAlumno", "Error al enviar el correo con los datos de la postulación.", ex);
            }
        }

        private string CuerpoPostulacionAlumno(string nombreResponsable, string apellidoResponsable, string alumno, string institucion, CursoApi curso, string codigoSeguimiento)
        {
            string responsable = nombreResponsable + " " + apellidoResponsable;

            string cuerpo = $@"
                <html>
                    <body style=""margin:0; padding:0; background-color:#F7F5FC; font-family:'Segoe UI', Roboto, Helvetica, Arial, sans-serif;"">
                        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#F7F5FC;"">
                            <tr>
                                <td align=""center"" style=""padding:32px 16px;"">
                                    <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""width:100%; max-width:600px;"">
                                        <tr>
                                            <td align=""center"" bgcolor=""#373A72"" style=""background-color:#373A72; border-radius:15px 15px 0 0; padding:32px 24px;"">
                                                <span style=""font-size:36px; font-weight:700; color:#FFFFFF;"">InstiRed</span>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td bgcolor=""#FFFFFF"" style=""background-color:#FFFFFF; border:1px solid #E4E0F0; border-top:none; border-radius:0 0 15px 15px; padding:40px 36px;"">
                                                <h1 style=""margin:0 0 16px 0; font-size:22px; color:#373A72;"">¡Tu postulación fue registrada!</h1>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    Hola, <strong>{nombreResponsable}</strong>. Recibimos correctamente la postulación de <strong>{alumno}</strong> en la institución <strong>{institucion}</strong>.
                                                </p>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    <strong>Curso solicitado:</strong> {curso.CicloLectivo} - {curso.Nivel} - {curso.Anio} {curso.Division} - {curso.Turno}
                                                </p>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    Tu postulación quedó pendiente de revisión por parte de la institución. Para consultar su estado, conservá el siguiente código de seguimiento:
                                                </p>

                                                <div style=""background-color:#F7F5FC; border:1px solid #E4E0F0; border-radius:10px; padding:20px; text-align:center; margin:24px 0;"">
                                                    <p style=""margin:0 0 8px 0; font-size:13px; color:#8D91D5;"">CÓDIGO DE SEGUIMIENTO</p>
                                                    <span style=""font-size:24px; font-weight:700; letter-spacing:1px; color:#373A72;"">{codigoSeguimiento}</span>
                                                </div>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    Recordá conservar este código para futuras consultas sobre la postulación.
                                                </p>

                                                <hr style=""border:none; border-top:1px solid #E4E0F0; margin:24px 0 20px 0;"">

                                                <p style=""font-size:13px; line-height:1.5; color:#8D91D5;"">
                                                    Este es un correo automático. Por favor, no respondas a esta dirección.
                                                </p>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </body>
                </html>";

            return cuerpo;
        }

        [Serializable]
        private class DocumentoAdjunto
        {
            public string Nombre { get; set; }
            public string Tipo { get; set; }
            public string Descripcion { get; set; }
            public byte[] Contenido { get; set; }
        }

        private class ResultadoPostulacion
        {
            public string CodigoSeguimiento { get; set; }
        }

        private class CursoApi
        {
            public int IdCurso { get; set; }
            public int IdInstitucion { get; set; }
            public int CicloLectivo { get; set; }
            public int? IdNivel { get; set; }
            public string Nivel { get; set; }
            public int? Anio { get; set; }
            public string NivelPersonalizado { get; set; }
            public string Division { get; set; }
            public int IdTurno { get; set; }
            public string Turno { get; set; }
            public int Capacidad { get; set; }
            public int Inscriptos { get; set; }
            public int VacantesDisponibles { get; set; }
        }

        private class AlumnoApi
        {
            public int? IdAlumno { get; set; }
            public string NombreAlumno { get; set; }
            public string ApellidoAlumno { get; set; }
        }

        // NOTAS

        // "async" --> Permite utilizar "await"
        // "Task" --> Operacion que se ejecuta de manera asincronica
        // "await" --> Permite esperar el resultado de una operacion asyncronica (Task) sin bloquear el
        //             hilo mientras espera. Si la respuesta vuelve, el metodo que lo llamo sigue.

    }
}

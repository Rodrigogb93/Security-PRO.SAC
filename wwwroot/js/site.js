// ============================================================
// SECURITY PRO S.A.C. - ERP Frontend
// ============================================================

const API = "/api";
let TOKEN = localStorage.getItem("token") || "";
let USER = JSON.parse(localStorage.getItem("user") || "null");

// ============ UTILIDADES ============
async function apiCall(endpoint, options = {}) {
  const headers = {
    "Content-Type": "application/json",
    ...(options.headers || {}),
  };
  if (TOKEN) headers["Authorization"] = `Bearer ${TOKEN}`;

  const res = await fetch(API + endpoint, { ...options, headers });
  if (res.status === 401) {
    logout();
    throw new Error("Sesión expirada");
  }
  if (!res.ok) {
    const err = await res
      .json()
      .catch(() => ({ message: "Error en el servidor" }));
    throw new Error(err.message || "Error en la petición");
  }
  if (res.headers.get("content-type")?.includes("application/json"))
    return res.json();
  return res;
}

function showToast(msg, type = "success") {
  const t = document.getElementById("toast");
  t.textContent = msg;
  t.className = "toast " + type;
  t.style.display = "block";
  setTimeout(() => (t.style.display = "none"), 3500);
}

function showModal(title, bodyHtml) {
  document.getElementById("modalTitle").textContent = title;
  document.getElementById("modalBody").innerHTML = bodyHtml;
  document.getElementById("modal").style.display = "flex";
}
function closeModal() {
  document.getElementById("modal").style.display = "none";
}

function escapeHtml(str) {
  if (str == null) return "";
  return String(str).replace(
    /[&<>"']/g,
    (c) =>
      ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" })[
        c
      ],
  );
}

function formatMoney(n) {
  return "S/ " + Number(n || 0).toFixed(2);
}
function formatDate(d) {
  return d ? new Date(d).toLocaleDateString("es-PE") : "-";
}

// ============ LOGIN ============
document.getElementById("loginForm").addEventListener("submit", async (e) => {
  e.preventDefault();
  const email = document.getElementById("loginEmail").value;
  const password = document.getElementById("loginPassword").value;
  try {
    const data = await apiCall("/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    });
    TOKEN = data.token;
    USER = data;
    localStorage.setItem("token", TOKEN);
    localStorage.setItem("user", JSON.stringify(USER));
    initApp();
  } catch (err) {
    const el = document.getElementById("loginError");
    el.textContent = err.message;
    el.style.display = "block";
  }
});

function logout() {
  localStorage.removeItem("token");
  localStorage.removeItem("user");
  TOKEN = "";
  USER = null;
  document.getElementById("appView").style.display = "none";
  document.getElementById("loginView").style.display = "flex";
}

// ============ MENÚ POR ROL ============
const MENU = {
  Administrador: [
    { id: "dashboard", icon: "📊", label: "Dashboard" },
    { id: "cotizaciones", icon: "📝", label: "Cotizaciones" },
    { id: "pedidos", icon: "📦", label: "Pedidos" },
    { id: "clientes", icon: "🏢", label: "Clientes" },
    { id: "catalogo", icon: "🦺", label: "Catálogo EPP" },
    { id: "kardex", icon: "📋", label: "Kardex" },
    { id: "vencimientos", icon: "⏰", label: "Vencimientos" },
    { id: "servicios", icon: "🔧", label: "Servicios Técnicos" },
    { id: "facturas", icon: "🧾", label: "Facturación" },
    { id: "cxc", icon: "💰", label: "Cuentas por Cobrar" },
    { id: "cxp", icon: "💸", label: "Cuentas por Pagar" },
    { id: "ssoma", icon: "📚", label: "SSOMA" },
    { id: "usuarios", icon: "👥", label: "Usuarios" },
    { id: "comisiones", icon: "🎯", label: "Comisiones" },
  ],
  Comercial: [
    { id: "cotizaciones", icon: "📝", label: "Cotizaciones" },
    { id: "pedidos", icon: "📦", label: "Pedidos" },
    { id: "clientes", icon: "🏢", label: "Clientes" },
    { id: "catalogo", icon: "🦺", label: "Catálogo EPP" },
  ],
  Almacén: [
    { id: "kardex", icon: "📋", label: "Kardex" },
    { id: "vencimientos", icon: "⏰", label: "Vencimientos" },
    { id: "pedidos", icon: "📦", label: "Pedidos" },
    { id: "catalogo", icon: "🦺", label: "Catálogo EPP" },
  ],
  "Servicios Técnicos": [
    { id: "servicios", icon: "🔧", label: "Servicios Técnicos" },
    { id: "clientes", icon: "🏢", label: "Clientes" },
  ],
  Finanzas: [
    { id: "facturas", icon: "🧾", label: "Facturación" },
    { id: "cxc", icon: "💰", label: "Cuentas por Cobrar" },
    { id: "cxp", icon: "💸", label: "Cuentas por Pagar" },
    { id: "comisiones", icon: "🎯", label: "Comisiones" },
  ],
  SSOMA: [
    { id: "ssoma", icon: "📚", label: "SSOMA" },
    { id: "catalogo", icon: "🦺", label: "Catálogo EPP" },
  ],
};

function initApp() {
  document.getElementById("loginView").style.display = "none";
  document.getElementById("appView").style.display = "block";
  document.getElementById("userInfo").textContent =
    `${USER.nombres} (${USER.rol})`;

  const menu = MENU[USER.rol] || MENU["Administrador"];
  const nav = document.getElementById("menuNav");
  nav.innerHTML = menu
    .map(
      (m) => `
    <a href="#" data-view="${m.id}" onclick="navigate('${m.id}');return false;">
      <span class="icon">${m.icon}</span><span>${m.label}</span>
    </a>`,
    )
    .join("");

  const first = menu[0]?.id || "dashboard";
  navigate(first);
}

function setActive(view) {
  document.querySelectorAll("#menuNav a").forEach((a) => {
    a.classList.toggle("active", a.dataset.view === view);
  });
}

// ============ NAVEGACIÓN ============
const VIEWS = {
  dashboard: renderDashboard,
  cotizaciones: renderCotizaciones,
  pedidos: renderPedidos,
  clientes: renderClientes,
  catalogo: renderCatalogo,
  kardex: renderKardex,
  vencimientos: renderVencimientos,
  servicios: renderServicios,
  facturas: renderFacturas,
  cxc: renderCxC,
  cxp: renderCxP,
  ssoma: renderSsoma,
  usuarios: renderUsuarios,
  comisiones: renderComisiones,
};

async function navigate(view) {
  setActive(view);
  const fn = VIEWS[view];
  if (fn) await fn();
}

// ============ DASHBOARD ============
async function renderDashboard() {
  const el = document.getElementById("mainContent");

  el.innerHTML = `
    <div class="page-header">
      <h2>Dashboard Ejecutivo</h2>
    </div>

    <div class="stats-grid" id="statsGrid">
      <div class="card">Cargando...</div>
    </div>

    <div id="alertasVencimiento"></div>
  `;

  try {
    const [facturas, cotizaciones, productos, servicios, vencimientos] =
      await Promise.all([
        apiCall("/facturas").catch(() => []),
        apiCall("/cotizaciones").catch(() => []),
        apiCall("/productos").catch(() => []),
        apiCall("/servicios").catch(() => []),

        // NUEVO
        apiCall("/kardex/semaforo-vencimiento").catch(() => []),
      ]);

    // =====================================================
    // ESTADÍSTICAS EXISTENTES
    // =====================================================

    const totalFacturado = facturas.reduce((s, f) => s + (f.total || 0), 0);

    const margen = totalFacturado * 0.35;

    document.getElementById("statsGrid").innerHTML = `
      <div class="stat-card">
        <div class="label">Facturación Total</div>

        <div class="value">
          ${formatMoney(totalFacturado)}
        </div>

        <div class="sub">
          ${facturas.length} comprobantes
        </div>
      </div>


      <div class="stat-card">
        <div class="label">
          Margen Bruto Estimado
        </div>

        <div class="value">
          ${formatMoney(margen)}
        </div>

        <div class="sub">
          35% promedio
        </div>
      </div>


      <div class="stat-card">
        <div class="label">
          Cotizaciones Activas
        </div>

        <div class="value">
          ${
            cotizaciones.filter(
              (c) => c.estado === "Cotizado" || c.estado === "En Negociación",
            ).length
          }
        </div>

        <div class="sub">
          Total: ${cotizaciones.length}
        </div>
      </div>


      <div class="stat-card">
        <div class="label">
          Productos en Catálogo
        </div>

        <div class="value">
          ${productos.length}
        </div>

        <div class="sub">
          EPPs homologados
        </div>
      </div>


      <div class="stat-card">
        <div class="label">
          Servicios Técnicos
        </div>

        <div class="value">
          ${servicios.length}
        </div>

        <div class="sub">
          Histórico
        </div>
      </div>
    `;

    // =====================================================
    // ALERTAS DE VENCIMIENTO
    // =====================================================

    const vencidos = vencimientos.filter((v) => v.diasRestantes <= 0);

    const proximos = vencimientos.filter(
      (v) => v.diasRestantes > 0 && v.diasRestantes <= 30,
    );

    const contenedorAlertas = document.getElementById("alertasVencimiento");

    // -----------------------------------------------------
    // NO HAY ALERTAS
    // -----------------------------------------------------

    if (vencidos.length === 0 && proximos.length === 0) {
      contenedorAlertas.innerHTML = `
        <div
          class="card"
          style="
            margin-top:20px;
            border-left:5px solid #22c55e;
          "
        >
          <strong>
            ✅ Sin alertas críticas de vencimiento
          </strong>

          <div style="margin-top:5px;">
            No existen lotes vencidos ni próximos
            a vencer durante los siguientes 30 días.
          </div>
        </div>
      `;

      return;
    }

    // -----------------------------------------------------
    // HAY ALERTAS
    // -----------------------------------------------------

    contenedorAlertas.innerHTML = `
      <div
        class="card"
        style="
          margin-top:20px;
          border-left:5px solid #ef4444;
        "
      >

        <h3 style="margin-top:0;">
          ⚠️ Alertas de inventario
        </h3>


        ${
          vencidos.length > 0
            ? `
              <div
                style="
                  padding:10px;
                  background:#fee2e2;
                  border-radius:8px;
                  margin-bottom:10px;
                "
              >
                🔴 <strong>
                  ${vencidos.length}
                  ${vencidos.length === 1 ? "lote vencido" : "lotes vencidos"}
                </strong>
                con stock disponible.
              </div>
            `
            : ""
        }


        ${
          proximos.length > 0
            ? `
              <div
                style="
                  padding:10px;
                  background:#fef3c7;
                  border-radius:8px;
                  margin-bottom:10px;
                "
              >
                ⚠️ <strong>
                  ${proximos.length}
                  ${proximos.length === 1 ? "lote está" : "lotes están"}
                  próximo${proximos.length === 1 ? "" : "s"}
                  a vencer.
                </strong>

                <br>

                Vencimiento dentro de los
                próximos 30 días.
              </div>
            `
            : ""
        }


        <div style="text-align:right;">
          <button
            class="btn-primary"
            onclick="navigate('vencimientos')"
          >
            Ver vencimientos
          </button>
        </div>

      </div>
    `;
  } catch (e) {
    showToast(e.message, "error");
  }
}
// ============ COTIZACIONES ============
async function renderCotizaciones() {
  const el = document.getElementById("mainContent");
  el.innerHTML = `
    <div class="page-header">
      <h2>Cotizaciones Comerciales</h2>
      <div class="actions">
        <button class="btn-primary" onclick="nuevaCotizacion()">+ Nueva Cotización</button>
      </div>
    </div>
    <div class="card"><div class="table-wrapper" id="cotTable">Cargando...</div></div>`;

  try {
    const data = await apiCall("/cotizaciones");
    document.getElementById("cotTable").innerHTML = `
      <table>
        <thead><tr><th>N°</th><th>Cliente</th><th>Asesor</th><th>Fecha</th><th>Total</th><th>Estado</th><th>Acciones</th></tr></thead>
        <tbody>${data
          .map(
            (c) => `
          <tr>
            <td><strong>${escapeHtml(c.numeroCotizacion)}</strong></td>
            <td>${escapeHtml(c.razonSocialCliente || "")}</td>
            <td>${escapeHtml(c.nombreAsesor || "")}</td>
            <td>${formatDate(c.fechaEmision)}</td>
            <td>${formatMoney(c.total)}</td>
            <td><span class="badge ${estadoBadge(c.estado)}">${escapeHtml(c.estado)}</span></td>
            <td>
              <button class="btn-secondary btn-sm" onclick="verCotizacion(${c.idCotizacion})">Ver</button>
              <button class="btn-secondary btn-sm" onclick="pdfCotizacion(${c.idCotizacion})">PDF</button>
              ${c.estado === "Cotizado" || c.estado === "En Negociación" ? `<button class="btn-primary btn-sm" onclick="concretarCotizacion(${c.idCotizacion})">Concretar</button>` : ""}
            </td>
          </tr>`,
          )
          .join("")}</tbody>
      </table>`;
  } catch (e) {
    showToast(e.message, "error");
  }
}

function estadoBadge(estado) {
  const map = {
    Cotizado: "badge-info",
    "En Negociación": "badge-warning",
    Concretada: "badge-success",
    Rechazada: "badge-danger",
    Anulada: "badge-secondary",
    Pendiente: "badge-warning",
    "En Picking": "badge-info",
    Despachado: "badge-info",
    Entregado: "badge-success",
    Facturado: "badge-success",
    Aceptado: "badge-success",
    Certificado: "badge-success",
    Pagado: "badge-success",
    Vencido: "badge-danger",
  };
  return map[estado] || "badge-secondary";
}

async function nuevaCotizacion() {
  const clientes = await apiCall("/clientes");
  const productos = await apiCall("/productos");

  showModal(
    "Nueva Cotización",
    `
    <div class="form-group"><label>Cliente</label>
      <select id="cotCliente">${clientes.map((c) => `<option value="${c.idCliente}">${escapeHtml(c.razonSocial)} - ${c.ruc}</option>`).join("")}</select>
    </div>
    <div class="form-group"><label>Validez (días)</label>
      <select id="cotValidez"><option value="15">15 días</option><option value="30" selected>30 días</option></select>
    </div>
    <h4 style="margin:16px 0 8px;color:var(--celeste-dark);">Productos</h4>
    <div id="cotLineas"></div>
    <button class="btn-secondary btn-sm" onclick="addLineaCotizacion()">+ Agregar producto</button>
    <div style="margin-top:20px;text-align:right;">
      <button class="btn-primary" onclick="guardarCotizacion()">Guardar y Emitir</button>
    </div>
  `,
  );
  window._productos = productos;
  window._lineas = [];
  addLineaCotizacion();
}

function addLineaCotizacion() {
  window._lineas.push({
    idProducto: window._productos[0]?.idProducto,
    cantidad: 1,
    precio: window._productos[0]?.precioVenta || 0,
    descuento: 0,
  });
  renderLineasCotizacion();
}

function renderLineasCotizacion() {
  const cont = document.getElementById("cotLineas");
  cont.innerHTML = window._lineas
    .map(
      (l, i) => `
    <div class="form-row-3" style="margin-bottom:10px;align-items:end;">
      <div><label>Producto</label>
        <select onchange="updateLinea(${i},'idProducto',this.value)">
          ${window._productos.map((p) => `<option value="${p.idProducto}" ${p.idProducto === l.idProducto ? "selected" : ""}>${escapeHtml(p.nombre)} (${p.sku})</option>`).join("")}
        </select>
      </div>
      <div><label>Cantidad</label><input type="number" min="1" value="${l.cantidad}" onchange="updateLinea(${i},'cantidad',this.value)"></div>
      <div><label>Precio Unit.</label><input type="number" step="0.01" value="${l.precio}" onchange="updateLinea(${i},'precio',this.value)"></div>
      <div><label>Descuento</label><input type="number" step="0.01" value="${l.descuento}" onchange="updateLinea(${i},'descuento',this.value)"></div>
      <button class="btn-danger btn-sm" onclick="removeLinea(${i})">✕</button>
    </div>`,
    )
    .join("");
}

function updateLinea(i, campo, valor) {
  window._lineas[i][campo] =
    campo === "idProducto" ? parseInt(valor) : parseFloat(valor);
  if (campo === "idProducto") {
    const p = window._productos.find((x) => x.idProducto === parseInt(valor));
    if (p) window._lineas[i].precio = p.precioVenta;
  }
  renderLineasCotizacion();
}
function removeLinea(i) {
  window._lineas.splice(i, 1);
  renderLineasCotizacion();
}

async function guardarCotizacion() {
  try {
    const body = {
      idCliente: parseInt(document.getElementById("cotCliente").value),
      validezDias: parseInt(document.getElementById("cotValidez").value),
      detalles: window._lineas.map((l) => ({
        idProducto: l.idProducto,
        cantidad: l.cantidad,
        precioUnitario: l.precio,
        descuentoVolumen: l.descuento,
      })),
    };
    await apiCall("/cotizaciones", {
      method: "POST",
      body: JSON.stringify(body),
    });
    showToast("Cotización creada exitosamente");
    closeModal();
    renderCotizaciones();
  } catch (e) {
    showToast(e.message, "error");
  }
}

async function verCotizacion(id) {
  try {
    const c = await apiCall(`/cotizaciones/${id}`);
    showModal(
      `Cotización ${c.numeroCotizacion}`,
      `
      <div class="form-row"><div><strong>Cliente:</strong> ${escapeHtml(c.razonSocialCliente)}</div>
      <div><strong>Fecha:</strong> ${formatDate(c.fechaEmision)}</div></div>
      <div class="form-row"><div><strong>Validez:</strong> ${c.validezDias} días</div>
      <div><strong>Estado:</strong> <span class="badge ${estadoBadge(c.estado)}">${c.estado}</span></div></div>
      <h4 style="margin:16px 0 8px;color:var(--celeste-dark);">Detalle</h4>
      <table><thead><tr><th>SKU</th><th>Producto</th><th>Cant.</th><th>P.Unit</th><th>Subtotal</th></tr></thead>
      <tbody>${c.detalles.map((d) => `<tr><td>${d.sku}</td><td>${escapeHtml(d.nombreProducto)}</td><td>${d.cantidad}</td><td>${formatMoney(d.precioUnitario)}</td><td>${formatMoney(d.subtotalLinea)}</td></tr>`).join("")}</tbody></table>
      <div style="text-align:right;margin-top:15px;">
        <div>Subtotal: ${formatMoney(c.subtotal)}</div>
        <div>IGV (18%): ${formatMoney(c.igv)}</div>
        <div style="font-size:18px;font-weight:700;color:var(--celeste-dark);">Total: ${formatMoney(c.total)}</div>
      </div>
    `,
    );
  } catch (e) {
    showToast(e.message, "error");
  }
}

async function pdfCotizacion(id) {
  try {
    const res = await fetch(`${API}/cotizaciones/${id}/pdf`, {
      method: "POST",
      headers: { Authorization: `Bearer ${TOKEN}` },
    });
    if (!res.ok) throw new Error("Error al generar PDF");
    const blob = await res.blob();
    const url = URL.createObjectURL(blob);
    window.open(url, "_blank");
  } catch (e) {
    showToast(e.message, "error");
  }
}

async function concretarCotizacion(id) {
  const oc = prompt("Ingrese el número de Orden de Compra del cliente:");
  if (!oc) return;
  try {
    await apiCall(`/cotizaciones/${id}/concretar`, {
      method: "PUT",
      body: JSON.stringify({ ordenCompraCliente: oc }),
    });
    showToast("Cotización concretada. Pedido generado y enviado a Almacén.");
    renderCotizaciones();
  } catch (e) {
    showToast(e.message, "error");
  }
}

// ============ PEDIDOS ============

async function renderPedidos() {
  const el = document.getElementById("mainContent");

  el.innerHTML = `
    <div class="page-header">
      <h2>Pedidos de Venta</h2>
    </div>

    <div class="card">
      <div class="table-wrapper" id="pedTable">
        Cargando...
      </div>
    </div>
  `;

  try {
    const data = await apiCall("/pedidos");

    document.getElementById("pedTable").innerHTML = `
      <table>
        <thead>
          <tr>
            <th>N° Pedido</th>
            <th>Cliente</th>
            <th>OC Cliente</th>
            <th>Fecha</th>
            <th>Total</th>
            <th>Estado</th>
            <th>Acciones</th>
          </tr>
        </thead>

        <tbody>
          ${
            data
              .map(
                (p) => `
              <tr>
                <td>
                  <strong>
                    ${escapeHtml(p.numeroPedido)}
                  </strong>
                </td>

                <td>
                  ${escapeHtml(p.razonSocialCliente || "")}
                </td>

                <td>
                  ${escapeHtml(p.ordenCompraCliente || "-")}
                </td>

                <td>
                  ${formatDate(p.fechaPedido)}
                </td>

                <td>
                  ${formatMoney(p.total)}
                </td>

                <td>
                  <span class="badge ${estadoBadge(p.estado)}">
                    ${escapeHtml(p.estado)}
                  </span>
                </td>

                <td>
                  <button
                    class="btn-secondary btn-sm"
                    onclick="verPedido(${p.idPedido})">
                    Ver
                  </button>

                  <button
                    class="btn-secondary btn-sm"
                    onclick="pdfPedido(${p.idPedido})">
                    PDF
                  </button>

                  ${
                    p.estado === "Pendiente"
                      ? `
                        <button
                          class="btn-primary btn-sm"
                          onclick="despacharPedido(
                            ${p.idPedido},
                            '${escapeHtml(p.numeroPedido)}'
                          )">
                          Despachar
                        </button>
                      `
                      : ""
                  }
                </td>
              </tr>
            `,
              )
              .join("") ||
            `
              <tr>
                <td colspan="7">
                  No hay pedidos registrados
                </td>
              </tr>
            `
          }
        </tbody>
      </table>
    `;
  } catch (e) {
    showToast(e.message, "error");
  }
}
async function despacharPedido(id, numeroPedido) {
  const confirmar = confirm(
    `¿Desea marcar el pedido ${numeroPedido} como despachado?`,
  );

  if (!confirmar) {
    return;
  }

  try {
    await apiCall(`/pedidos/${id}/estado`, {
      method: "PUT",
      body: JSON.stringify("Despachado"),
    });

    showToast(`Pedido ${numeroPedido} marcado como despachado`);

    renderPedidos();
  } catch (e) {
    showToast(e.message, "error");
  }
}

// ============ VER PEDIDO ============

async function verPedido(id) {
  try {
    const p = await apiCall(`/pedidos/${id}`);

    const detalles = (p.detalles || [])
      .map(
        (d) => `
          <tr>
            <td>${escapeHtml(d.sku || "")}</td>
            <td>${escapeHtml(d.nombreProducto || "")}</td>
            <td>${d.cantidad}</td>
            <td>${formatMoney(d.precioUnitario)}</td>
            <td>${formatMoney(d.subtotalLinea)}</td>
          </tr>
        `,
      )
      .join("");

    showModal(
      `Pedido ${p.numeroPedido}`,
      `
        <div class="form-row">
          <div>
            <strong>Cliente:</strong>
            ${escapeHtml(p.razonSocialCliente || "")}
          </div>

          <div>
            <strong>Fecha:</strong>
            ${formatDate(p.fechaPedido)}
          </div>
        </div>

        <div class="form-row">
          <div>
            <strong>Orden de Compra:</strong>
            ${escapeHtml(p.ordenCompraCliente || "-")}
          </div>

          <div>
            <strong>Estado:</strong>
            <span class="badge ${estadoBadge(p.estado)}">
              ${escapeHtml(p.estado || "")}
            </span>
          </div>
        </div>

        ${
          p.fechaEntregaProgramada
            ? `
              <div style="margin-top:10px;">
                <strong>Entrega programada:</strong>
                ${formatDate(p.fechaEntregaProgramada)}
              </div>
            `
            : ""
        }

        ${
          p.direccionEntrega
            ? `
              <div style="margin-top:6px;">
                <strong>Dirección de entrega:</strong>
                ${escapeHtml(p.direccionEntrega)}
              </div>
            `
            : ""
        }

        <h4 style="margin:16px 0 8px;color:var(--celeste-dark);">
          Detalle del Pedido
        </h4>

        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>SKU</th>
                <th>Producto</th>
                <th>Cant.</th>
                <th>P. Unit.</th>
                <th>Subtotal</th>
              </tr>
            </thead>

            <tbody>
              ${
                detalles ||
                `
                  <tr>
                    <td colspan="5">
                      Sin productos registrados
                    </td>
                  </tr>
                `
              }
            </tbody>
          </table>
        </div>

        <div style="text-align:right;margin-top:15px;">
          <div>
            Subtotal: ${formatMoney(p.subtotal)}
          </div>

          <div>
            IGV (18%): ${formatMoney(p.igv)}
          </div>

          <div style="
            font-size:18px;
            font-weight:700;
            color:var(--celeste-dark);
            margin-top:5px;
          ">
            Total: ${formatMoney(p.total)}
          </div>
        </div>

        <div style="text-align:right;margin-top:18px;">
          <button
            class="btn-secondary"
            onclick="pdfPedido(${p.idPedido})">
            📄 PDF
          </button>
        </div>
      `,
    );
  } catch (e) {
    showToast(e.message, "error");
  }
}

// ============ PDF PEDIDO ============

async function pdfPedido(id) {
  try {
    const res = await fetch(`${API}/pedidos/${id}/pdf`, {
      method: "GET",

      headers: {
        Authorization: `Bearer ${TOKEN}`,
      },
    });

    if (!res.ok) {
      throw new Error("Error al generar el PDF del pedido");
    }

    const blob = await res.blob();

    const url = URL.createObjectURL(blob);

    // Abrimos el PDF en otra pestaña
    window.open(url, "_blank");

    // Liberamos la memoria después de 1 minuto
    setTimeout(() => {
      URL.revokeObjectURL(url);
    }, 60000);
  } catch (e) {
    showToast(e.message, "error");
  }
}

async function pdfPedido(id) {
  try {
    const res = await fetch(`${API}/pedidos/${id}/pdf`, {
      headers: {
        Authorization: `Bearer ${TOKEN}`,
      },
    });

    if (!res.ok) throw new Error("Error al generar el PDF del pedido");

    const blob = await res.blob();

    const url = URL.createObjectURL(blob);

    const a = document.createElement("a");

    a.href = url;
    a.download = `Pedido_${id}.pdf`;

    document.body.appendChild(a);
    a.click();
    a.remove();

    URL.revokeObjectURL(url);

    showToast("Pedido descargado correctamente");
  } catch (e) {
    showToast(e.message, "error");
  }
}

// ============ CLIENTES ============
async function renderClientes() {
  const el = document.getElementById("mainContent");
  el.innerHTML = `
    <div class="page-header"><h2>Clientes Corporativos</h2>
      <div class="actions">
        <input type="text" id="searchCliente" placeholder="Buscar por RUC o Razón Social..." style="padding:8px 12px;border:1.5px solid var(--gray-300);border-radius:8px;">
        <button class="btn-primary" onclick="nuevoCliente()">+ Nuevo Cliente</button>
      </div>
    </div>
    <div class="card"><div class="table-wrapper" id="cliTable">Cargando...</div></div>`;
  await cargarClientes();

  document
    .getElementById("searchCliente")
    .addEventListener("input", debounce(cargarClientes, 300));
}

async function cargarClientes() {
  const search = document.getElementById("searchCliente")?.value || "";
  try {
    const data = await apiCall(
      `/clientes?search=${encodeURIComponent(search)}`,
    );
    document.getElementById("cliTable").innerHTML = `
      <table><thead><tr><th>RUC</th><th>Razón Social</th><th>Sector</th><th>Cond. Pago</th><th>Línea Crédito</th><th>Acciones</th></tr></thead>
      <tbody>${data
        .map(
          (c) => `<tr>
        <td>${c.ruc}</td>
        <td><strong>${escapeHtml(c.razonSocial)}</strong></td>
        <td>${escapeHtml(c.sectorEconomico || "-")}</td>
        <td>${escapeHtml(c.condicionesPago)}</td>
        <td>${formatMoney(c.lineaCredito)}</td>
        <td><button class="btn-secondary btn-sm" onclick="editarCliente(${c.idCliente})">Editar</button></td>
      </tr>`,
        )
        .join("")}</tbody></table>`;
  } catch (e) {
    showToast(e.message, "error");
  }
}

function debounce(fn, ms) {
  let t;
  return (...a) => {
    clearTimeout(t);
    t = setTimeout(() => fn(...a), ms);
  };
}

async function nuevoCliente() {
  showModal("Nuevo Cliente", formCliente({}));
}
async function editarCliente(id) {
  const c = await apiCall(`/clientes/${id}`);
  showModal("Editar Cliente", formCliente(c));
}

function formCliente(c) {
  return `
    <div class="form-row">
      <div class="form-group"><label>RUC</label><input id="cRuc" value="${c.ruc || ""}" maxlength="11"></div>
      <div class="form-group"><label>Razón Social</label><input id="cRazon" value="${escapeHtml(c.razonSocial || "")}"></div>
    </div>
    <div class="form-group"><label>Dirección Fiscal</label><input id="cDir" value="${escapeHtml(c.direccionFiscal || "")}"></div>
    <div class="form-row">
      <div class="form-group"><label>Teléfono</label><input id="cTel" value="${c.telefono || ""}"></div>
      <div class="form-group"><label>Email</label><input id="cEmail" value="${c.emailContacto || ""}"></div>
    </div>
    <div class="form-row">
      <div class="form-group"><label>Sector Económico</label>
        <select id="cSector">
          ${["Gran Mineria", "Construcción e Infraestructura", "Industria Manufacturera", "Servicios Técnicos"].map((s) => `<option ${c.sectorEconomico === s ? "selected" : ""}>${s}</option>`).join("")}
        </select>
      </div>
      <div class="form-group"><label>Condiciones de Pago</label>
        <select id="cCond">
          ${["Contado", "30 días", "60 días", "90 días"].map((s) => `<option ${c.condicionesPago === s ? "selected" : ""}>${s}</option>`).join("")}
        </select>
      </div>
    </div>
    <div class="form-group"><label>Línea de Crédito</label><input type="number" step="0.01" id="cLinea" value="${c.lineaCredito || 0}"></div>
    <div class="form-group"><label><input type="checkbox" id="cConvenio" ${c.convenioSuministro ? "checked" : ""}> Convenio de suministro continuo</label></div>
    <div style="text-align:right;"><button class="btn-primary" onclick="guardarCliente(${c.idCliente || 0})">Guardar</button></div>`;
}

async function guardarCliente(id) {
  const body = {
    ruc: document.getElementById("cRuc").value,
    razonSocial: document.getElementById("cRazon").value,
    direccionFiscal: document.getElementById("cDir").value,
    telefono: document.getElementById("cTel").value,
    emailContacto: document.getElementById("cEmail").value,
    sectorEconomico: document.getElementById("cSector").value,
    condicionesPago: document.getElementById("cCond").value,
    lineaCredito: parseFloat(document.getElementById("cLinea").value),
    convenioSuministro: document.getElementById("cConvenio").checked,
  };
  try {
    if (id)
      await apiCall(`/clientes/${id}`, {
        method: "PUT",
        body: JSON.stringify(body),
      });
    else
      await apiCall("/clientes", {
        method: "POST",
        body: JSON.stringify(body),
      });
    showToast("Cliente guardado");
    closeModal();
    cargarClientes();
  } catch (e) {
    showToast(e.message, "error");
  }
}

// ============ CATÁLOGO EPP ============
async function renderCatalogo() {
  const el = document.getElementById("mainContent");
  el.innerHTML = `
    <div class="page-header"><h2>Catálogo Digital de EPPs</h2>
      <div class="actions">
        <input type="text" id="searchProd" placeholder="Buscar..." style="padding:8px 12px;border:1.5px solid var(--gray-300);border-radius:8px;">
        ${USER.rol === "Administrador" || USER.rol === "Almacén" ? '<button class="btn-primary" onclick="nuevoProducto()">+ Nuevo Producto</button>' : ""}
      </div>
    </div>
    <div class="card"><div class="table-wrapper" id="prodTable">Cargando...</div></div>`;
  await cargarProductos();
  document
    .getElementById("searchProd")
    .addEventListener("input", debounce(cargarProductos, 300));
}

async function cargarProductos() {
  try {
    const data = await apiCall("/productos");
    document.getElementById("prodTable").innerHTML = `
      <table><thead><tr><th>SKU</th><th>Producto</th><th>Categoría</th><th>Marca</th><th>Normativa</th><th>Talla</th><th>P. Venta</th><th>Acciones</th></tr></thead>
      <tbody>${data
        .map(
          (p) => `<tr>
        <td><strong>${escapeHtml(p.sku)}</strong></td>
        <td>${escapeHtml(p.nombre)}</td>
        <td><span class="badge badge-info">${escapeHtml(p.nombreCategoria || "")}</span></td>
        <td>${escapeHtml(p.marca || "")}</td>
        <td>${escapeHtml(p.normativa || "")}</td>
        <td>${escapeHtml(p.talla || "")}</td>
        <td>${formatMoney(p.precioVenta)}</td>
        <td>${USER.rol === "Administrador" || USER.rol === "Almacén" ? `<button class="btn-secondary btn-sm" onclick="editarProducto(${p.idProducto})">Editar</button>` : ""}</td>
      </tr>`,
        )
        .join("")}</tbody></table>`;
  } catch (e) {
    showToast(e.message, "error");
  }
}

async function nuevoProducto() {
  const categorias = await apiCall("/productos/categorias");
  showModal("Nuevo Producto", formProducto({}, categorias));
}
async function editarProducto(id) {
  const [p, categorias] = await Promise.all([
    apiCall(`/productos/${id}`),
    apiCall("/productos/categorias"),
  ]);
  showModal("Editar Producto", formProducto(p, categorias));
}

function formProducto(p, categorias) {
  return `
    <div class="form-row">
      <div class="form-group"><label>SKU</label><input id="pSku" value="${escapeHtml(p.sku || "")}"></div>
      <div class="form-group"><label>Categoría</label>
        <select id="pCat">${categorias.map((c) => `<option value="${c.idCategoria}" ${p.idCategoria === c.idCategoria ? "selected" : ""}>${escapeHtml(c.nombre)}</option>`).join("")}</select>
      </div>
    </div>
    <div class="form-group"><label>Nombre</label><input id="pNombre" value="${escapeHtml(p.nombre || "")}"></div>
    <div class="form-row">
      <div class="form-group"><label>Marca</label><input id="pMarca" value="${escapeHtml(p.marca || "")}"></div>
      <div class="form-group"><label>Modelo</label><input id="pModelo" value="${escapeHtml(p.modelo || "")}"></div>
    </div>
    <div class="form-row">
      <div class="form-group"><label>Normativa</label><input id="pNorm" value="${escapeHtml(p.normativa || "")}" placeholder="ANSI Z87.1, NIOSH..."></div>
      <div class="form-group"><label>Talla</label><input id="pTalla" value="${escapeHtml(p.talla || "")}"></div>
    </div>
    <div class="form-row-3">
      <div class="form-group"><label>P. Compra</label><input type="number" step="0.01" id="pPc" value="${p.precioCompra || 0}"></div>
      <div class="form-group"><label>P. Venta</label><input type="number" step="0.01" id="pPv" value="${p.precioVenta || 0}"></div>
      <div class="form-group"><label>Stock Mín.</label><input type="number" id="pSm" value="${p.stockMinimo || 10}"></div>
    </div>
    <div style="text-align:right;"><button class="btn-primary" onclick="guardarProducto(${p.idProducto || 0})">Guardar</button></div>`;
}

async function guardarProducto(id) {
  const body = {
    idCategoria: parseInt(document.getElementById("pCat").value),
    sku: document.getElementById("pSku").value,
    nombre: document.getElementById("pNombre").value,
    marca: document.getElementById("pMarca").value,
    modelo: document.getElementById("pModelo").value,
    normativa: document.getElementById("pNorm").value,
    talla: document.getElementById("pTalla").value,
    precioCompra: parseFloat(document.getElementById("pPc").value),
    precioVenta: parseFloat(document.getElementById("pPv").value),
    stockMinimo: parseInt(document.getElementById("pSm").value),
    stockMaximo: 1000,
  };
  try {
    if (id)
      await apiCall(`/productos/${id}`, {
        method: "PUT",
        body: JSON.stringify(body),
      });
    else
      await apiCall("/productos", {
        method: "POST",
        body: JSON.stringify(body),
      });
    showToast("Producto guardado");
    closeModal();
    cargarProductos();
  } catch (e) {
    showToast(e.message, "error");
  }
}

// ============ KARDEX ============
async function renderKardex() {
  const el = document.getElementById("mainContent");
  el.innerHTML = `
    <div class="page-header"><h2>Kardex de Inventario</h2>
      <div class="actions">
        <button class="btn-secondary" onclick="exportarKardex()">📥 Excel</button>
        <button class="btn-primary" onclick="nuevoMovimiento()">+ Registrar Movimiento</button>
      </div>
    </div>
    <div class="card"><div class="table-wrapper" id="kdxTable">Cargando...</div></div>`;
  try {
    const data = await apiCall("/kardex");
    document.getElementById("kdxTable").innerHTML = `
      <table><thead><tr><th>SKU</th><th>Producto</th><th>Lote</th><th>Tipo</th><th>Cant.</th><th>Saldo Ant.</th><th>Saldo Act.</th><th>Fecha</th><th>Doc.</th></tr></thead>
      <tbody>${data
        .map(
          (k) => `<tr>
        <td>${escapeHtml(k.sku || "")}</td>
        <td>${escapeHtml(k.nombreProducto || "")}</td>
        <td>${escapeHtml(k.numeroLote || "-")}</td>
        <td><span class="badge ${k.tipoMovimiento === "Entrada" ? "badge-success" : "badge-warning"}">${escapeHtml(k.tipoMovimiento)}</span></td>
        <td>${k.cantidad}</td>
        <td>${k.saldoAnterior}</td>
        <td><strong>${k.saldoActual}</strong></td>
        <td>${new Date(k.fechaMovimiento).toLocaleString("es-PE")}</td>
        <td>${escapeHtml(k.documentoReferencia || "-")}</td>
      </tr>`,
        )
        .join("")}</tbody></table>`;
  } catch (e) {
    showToast(e.message, "error");
  }
}

async function nuevoMovimiento() {
  try {
    const productos = await apiCall("/productos");

    showModal(
      "Registrar Movimiento de Kardex",
      `
        <div class="form-group">
          <label>Producto</label>
          <select id="kProd" onchange="actualizarFormularioKardex()">
            ${productos
              .map(
                (p) => `
                  <option value="${p.idProducto}">
                    ${escapeHtml(p.nombre)} (${escapeHtml(p.sku)})
                  </option>
                `,
              )
              .join("")}
          </select>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>Tipo de movimiento</label>
            <select id="kTipo" onchange="actualizarFormularioKardex()">
              <option value="Entrada">Entrada</option>
              <option value="Salida">Salida</option>
              <option value="Baja">Baja</option>
              <option value="Merma">Merma</option>
              <option value="Ajuste">Ajuste</option>
            </select>
          </div>

          <div class="form-group">
            <label>Cantidad</label>
            <input type="number" id="kCant" value="1" min="1">
          </div>
        </div>

        <div id="kardexLoteContainer"></div>

        <div class="form-group">
          <label>Documento de referencia</label>
          <input id="kDoc" placeholder="Factura, OC, Guía...">
        </div>

        <div class="form-group">
          <label>Observaciones</label>
          <textarea id="kObs" rows="2"></textarea>
        </div>

        <div style="text-align:right;">
          <button class="btn-primary" onclick="guardarMovimiento()">
            Registrar
          </button>
        </div>
      `,
    );

    await actualizarFormularioKardex();
  } catch (e) {
    showToast(e.message, "error");
  }
}
async function actualizarFormularioKardex() {
  const tipo = document.getElementById("kTipo").value;
  const idProducto = parseInt(document.getElementById("kProd").value);

  const cont = document.getElementById("kardexLoteContainer");

  // ENTRADA: permite escribir un lote nuevo
  if (tipo === "Entrada") {
    cont.innerHTML = `
      <div class="form-row">
        <div class="form-group">
          <label>N° Lote</label>
          <input
            id="kLote"
            placeholder="Ej: LOTE-2026-001">
        </div>

        <div class="form-group">
          <label>Fecha de vencimiento</label>
          <input
            type="date"
            id="kFechaVenc">
        </div>
      </div>
    `;

    return;
  }

  // SALIDA / BAJA / MERMA:
  // debe seleccionar un lote que ya existe.
  if (tipo === "Salida" || tipo === "Baja" || tipo === "Merma") {
    try {
      const lotes = await apiCall(`/kardex/lotes/${idProducto}`);

      if (!lotes.length) {
        cont.innerHTML = `
          <div class="form-group">
            <label>Lote</label>

            <div style="
              padding:10px;
              background:#fee2e2;
              border-radius:8px;
              color:#991b1b;
            ">
              No existen lotes con stock disponible
              para este producto.
            </div>
          </div>
        `;

        return;
      }

      cont.innerHTML = `
        <div class="form-group">
          <label>Lote de salida</label>

          <select id="kIdLote">
            ${lotes
              .map(
                (l) => `
                  <option value="${l.idLote}">
                    ${escapeHtml(l.numeroLote)}
                    — Stock: ${l.cantidadActual}
                    ${
                      l.fechaVencimiento
                        ? `— Vence: ${formatDate(l.fechaVencimiento)}`
                        : ""
                    }
                  </option>
                `,
              )
              .join("")}
          </select>
        </div>
      `;

      return;
    } catch (e) {
      cont.innerHTML = `
        <div style="color:#991b1b;">
          Error al cargar los lotes
        </div>
      `;
      return;
    }
  }

  // AJUSTE
  cont.innerHTML = `
    <div style="
      padding:10px;
      background:#fef3c7;
      border-radius:8px;
      margin-bottom:12px;
    ">
      El ajuste se registrará por ahora
      como un incremento de stock.
    </div>
  `;
}
async function guardarMovimiento() {
  const tipo = document.getElementById("kTipo").value;

  const body = {
    idProducto: parseInt(document.getElementById("kProd").value),

    tipoMovimiento: tipo,

    cantidad: parseInt(document.getElementById("kCant").value),

    documentoReferencia: document.getElementById("kDoc").value,

    observaciones: document.getElementById("kObs").value,

    idLote: null,
    numeroLote: null,
    fechaVencimiento: null,
  };

  // Si es entrada, enviamos el nuevo lote.
  if (tipo === "Entrada") {
    body.numeroLote = document.getElementById("kLote")?.value || null;

    body.fechaVencimiento =
      document.getElementById("kFechaVenc")?.value || null;
  }

  // Si sale mercadería, enviamos el ID del lote existente.
  if (tipo === "Salida" || tipo === "Baja" || tipo === "Merma") {
    const selector = document.getElementById("kIdLote");

    if (!selector) {
      showToast("No existe un lote disponible para este movimiento", "error");
      return;
    }

    body.idLote = parseInt(selector.value);
  }

  if (!body.cantidad || body.cantidad <= 0) {
    showToast("Ingrese una cantidad válida", "error");
    return;
  }

  try {
    await apiCall("/kardex/movimiento", {
      method: "POST",
      body: JSON.stringify(body),
    });

    showToast("Movimiento registrado correctamente");

    closeModal();
    renderKardex();
  } catch (e) {
    showToast(e.message, "error");
  }
}

async function exportarKardex() {
  const res = await fetch(`${API}/kardex/exportar-excel`, {
    headers: { Authorization: `Bearer ${TOKEN}` },
  });
  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = "Kardex.xlsx";
  a.click();
}

// ============ VENCIMIENTOS ============
async function renderVencimientos() {
  const el = document.getElementById("mainContent");
  el.innerHTML = `<div class="page-header"><h2>Semáforo de Vencimiento de Filtros y Cartuchos</h2></div>
    <div class="card"><div class="table-wrapper" id="vencTable">Cargando...</div></div>`;
  try {
    const data = await apiCall("/kardex/semaforo-vencimiento");
    document.getElementById("vencTable").innerHTML = `
      <table><thead><tr><th>Semáforo</th><th>SKU</th><th>Producto</th><th>Lote</th><th>Vencimiento</th><th>Días Restantes</th></tr></thead>
      <tbody>${
        data
          .map(
            (v) => `<tr>
        <td><span class="semaforo ${v.color.toLowerCase()}"></span> ${v.color}</td>
        <td>${escapeHtml(v.sku)}</td>
        <td>${escapeHtml(v.producto)}</td>
        <td>${escapeHtml(v.numeroLote)}</td>
        <td>${formatDate(v.fechaVencimiento)}</td>
        <td><strong>${v.diasRestantes}</strong></td>
      </tr>`,
          )
          .join("") || '<tr><td colspan="6">Sin vencimientos próximos</td></tr>'
      }</tbody></table>`;
  } catch (e) {
    showToast(e.message, "error");
  }
}

// ============ SERVICIOS TÉCNICOS ============
async function renderServicios() {
  const el = document.getElementById("mainContent");
  el.innerHTML = `
    <div class="page-header"><h2>Servicios Técnicos</h2>
      <div class="actions"><button class="btn-primary" onclick="nuevoServicio()">+ Nuevo Servicio</button></div>
    </div>
    <div class="card"><div class="table-wrapper" id="servTable">Cargando...</div></div>`;
  try {
    const data = await apiCall("/servicios");
    document.getElementById("servTable").innerHTML = `
      <table><thead><tr><th>ID</th><th>Tipo</th><th>Cliente</th><th>Fecha</th><th>Estado</th><th>Acciones</th></tr></thead>
      <tbody>${data
        .map(
          (s) => `<tr>
        <td>#${s.idServicio}</td>
        <td><span class="badge badge-info">${escapeHtml(s.tipoServicio)}</span></td>
        <td>${escapeHtml(s.razonSocialCliente || "")}</td>
        <td>${formatDate(s.fechaServicio)}</td>
        <td><span class="badge ${estadoBadge(s.estado)}">${escapeHtml(s.estado)}</span></td>
        <td><button class="btn-secondary btn-sm" onclick="certificadoServicio(${s.idServicio})">📄 Certificado</button></td>
      </tr>`,
        )
        .join("")}</tbody></table>`;
  } catch (e) {
    showToast(e.message, "error");
  }
}

async function nuevoServicio() {
  const clientes = await apiCall("/clientes");
  showModal(
    "Nuevo Servicio Técnico",
    `
    <div class="form-group"><label>Tipo de Servicio</label>
      <select id="sTipo" onchange="cambiarFormServicio()">
        <option value="Recarga_Extintor">Recarga de Extintor</option>
        <option value="Inspeccion_Arnes">Inspección de Arnés</option>
        <option value="Fit_Test">Fit Test de Respirador</option>
      </select>
    </div>
    <div class="form-group"><label>Cliente</label>
      <select id="sCliente">${clientes.map((c) => `<option value="${c.idCliente}">${escapeHtml(c.razonSocial)}</option>`).join("")}</select>
    </div>
    <div id="sDetalle"></div>
    <div style="text-align:right;"><button class="btn-primary" onclick="guardarServicio()">Registrar Servicio</button></div>`,
  );
  cambiarFormServicio();
}

function cambiarFormServicio() {
  const tipo = document.getElementById("sTipo").value;
  const cont = document.getElementById("sDetalle");
  if (tipo === "Recarga_Extintor") {
    cont.innerHTML = `
      <div class="form-row">
        <div class="form-group"><label>N° Serie Cilindro</label><input id="eSerie"></div>
        <div class="form-group"><label>Tipo Agente</label>
          <select id="eAgente"><option>PQS</option><option>CO2</option><option>Acetato de Potasio</option><option>Solkaflam</option></select>
        </div>
      </div>
      <div class="form-row-3">
        <div class="form-group"><label>Capacidad Kg</label><input type="number" step="0.01" id="eCap" value="6"></div>
        <div class="form-group"><label>Presión Manómetro</label><input type="number" step="0.01" id="ePres" value="150"></div>
        <div class="form-group"><label>Peso Recargado</label><input type="number" step="0.01" id="ePeso" value="6"></div>
      </div>`;
  } else if (tipo === "Inspeccion_Arnes") {
    cont.innerHTML = `
      <div class="form-row">
        <div class="form-group"><label>N° Serie Arnés</label><input id="aSerie"></div>
        <div class="form-group"><label>Marca</label><input id="aMarca" value="MSA"></div>
      </div>
      <div class="form-group"><label>Modelo</label><input id="aModelo"></div>
      <div style="display:grid;grid-template-columns:1fr 1fr;gap:8px;">
        <label><input type="checkbox" id="aReatas" checked> Reatas / Cintas OK</label>
        <label><input type="checkbox" id="aCosturas" checked> Costuras OK</label>
        <label><input type="checkbox" id="aAnillos" checked> Anillos D OK</label>
        <label><input type="checkbox" id="aHebillas" checked> Hebillas OK</label>
        <label><input type="checkbox" id="aAbsorb" checked> Absorbedor OK</label>
      </div>
      <div class="form-group"><label>Resultado</label>
        <select id="aResultado"><option>Aprobado</option><option>Rechazado</option></select>
      </div>`;
  } else {
    cont.innerHTML = `
      <div class="form-row">
        <div class="form-group"><label>Tipo Prueba</label>
          <select id="fTipo"><option>Cualitativa</option><option>Cuantitativa</option></select>
        </div>
        <div class="form-group"><label>Resultado</label>
          <select id="fRes"><option>Aprobado</option><option>Rechazado</option></select>
        </div>
      </div>
      <div class="form-row">
        <div class="form-group"><label>Marca Respirador</label><input id="fMarca" value="3M"></div>
        <div class="form-group"><label>Modelo</label><input id="fModelo" value="6200"></div>
      </div>`;
  }
}

async function guardarServicio() {
  const tipo = document.getElementById("sTipo").value;
  const body = {
    tipoServicio: tipo,
    idCliente: parseInt(document.getElementById("sCliente").value),
  };
  if (tipo === "Recarga_Extintor") {
    body.extintores = [
      {
        numeroSerieCilindro: document.getElementById("eSerie").value,
        tipoAgente: document.getElementById("eAgente").value,
        capacidadKg: parseFloat(document.getElementById("eCap").value),
        presionManometro: parseFloat(document.getElementById("ePres").value),
        pesoAgenteRecargado: parseFloat(document.getElementById("ePeso").value),
      },
    ];
  } else if (tipo === "Inspeccion_Arnes") {
    body.arneses = [
      {
        numeroSerieArnes: document.getElementById("aSerie").value,
        marca: document.getElementById("aMarca").value,
        modelo: document.getElementById("aModelo").value,
        checklistReatas: document.getElementById("aReatas").checked,
        checklistCosturas: document.getElementById("aCosturas").checked,
        checklistAnillosD: document.getElementById("aAnillos").checked,
        checklistHebillas: document.getElementById("aHebillas").checked,
        checklistAbsorbedores: document.getElementById("aAbsorb").checked,
        resultadoChecklist: document.getElementById("aResultado").value,
      },
    ];
  }
  try {
    await apiCall("/servicios", { method: "POST", body: JSON.stringify(body) });
    showToast("Servicio registrado");
    closeModal();
    renderServicios();
  } catch (e) {
    showToast(e.message, "error");
  }
}

async function certificadoServicio(id) {
  const res = await fetch(`${API}/servicios/${id}/certificado`, {
    method: "POST",
    headers: { Authorization: `Bearer ${TOKEN}` },
  });
  const blob = await res.blob();
  window.open(URL.createObjectURL(blob), "_blank");
}

// ============ FACTURAS ============
async function renderFacturas() {
  const el = document.getElementById("mainContent");
  el.innerHTML = `
    <div class="page-header"><h2>Facturación Electrónica SUNAT</h2>
      <div class="actions"><button class="btn-primary" onclick="nuevaFactura()">+ Emitir Factura</button></div>
    </div>
    <div class="card"><div class="table-wrapper" id="factTable">Cargando...</div></div>`;
  try {
    const data = await apiCall("/facturas");
    document.getElementById("factTable").innerHTML = `
      <table><thead><tr><th>Serie-Número</th><th>Cliente</th><th>Fecha</th><th>Subtotal</th><th>IGV</th><th>Total</th><th>Estado SUNAT</th></tr></thead>
      <tbody>${
        data
          .map(
            (f) => `<tr>
        <td><strong>${f.serie}-${f.numero}</strong></td>
        <td>${escapeHtml(f.razonSocialCliente || "")}</td>
        <td>${formatDate(f.fechaEmision)}</td>
        <td>${formatMoney(f.subtotal)}</td>
        <td>${formatMoney(f.igv)}</td>
        <td><strong>${formatMoney(f.total)}</strong></td>
        <td><span class="badge ${estadoBadge(f.estadoSunat)}">${escapeHtml(f.estadoSunat)}</span></td>
      </tr>`,
          )
          .join("") || '<tr><td colspan="7">Sin facturas emitidas</td></tr>'
      }</tbody></table>`;
  } catch (e) {
    showToast(e.message, "error");
  }
}

async function nuevaFactura() {
  const pedidos = await apiCall("/pedidos");
  const pendientes = pedidos.filter(
    (p) => p.estado === "Entregado" || p.estado === "Despachado",
  );
  if (pendientes.length === 0) {
    showToast("No hay pedidos pendientes de facturar", "error");
    return;
  }

  showModal(
    "Emitir Factura Electrónica",
    `
    <div class="form-group"><label>Pedido a Facturar</label>
      <select id="fPedido">${pendientes.map((p) => `<option value="${p.idPedido}">${escapeHtml(p.numeroPedido)} - ${escapeHtml(p.razonSocialCliente || "")} - ${formatMoney(p.total)}</option>`).join("")}</select>
    </div>
    <div class="form-group"><label>Condición de Pago</label>
      <select id="fCond"><option value="Contado">Contado</option><option value="Crédito_30">Crédito 30 días</option><option value="Crédito_60">Crédito 60 días</option></select>
    </div>
    <div style="text-align:right;"><button class="btn-primary" onclick="emitirFactura()">Emitir ante SUNAT</button></div>`,
  );
}

async function emitirFactura() {
  const body = {
    idPedido: parseInt(document.getElementById("fPedido").value),
    condicionPago: document.getElementById("fCond").value,
  };
  try {
    const res = await apiCall("/facturas/emitir", {
      method: "POST",
      body: JSON.stringify(body),
    });
    showToast(`Factura ${res.numero} emitida - Estado: ${res.estado}`);
    closeModal();
    renderFacturas();
  } catch (e) {
    showToast(e.message, "error");
  }
}

// ============ CxC ============
async function renderCxC() {
  const el = document.getElementById("mainContent");
  el.innerHTML = `<div class="page-header"><h2>Cuentas por Cobrar</h2></div>
    <div class="card"><div class="table-wrapper" id="cxcTable">Cargando...</div></div>`;
  try {
    const data = await apiCall("/cuentas-cobrar");
    document.getElementById("cxcTable").innerHTML = `
      <table><thead><tr><th>Factura</th><th>Cliente</th><th>Total</th><th>Saldo</th><th>Vencimiento</th><th>Estado</th><th>Acciones</th></tr></thead>
      <tbody>${
        data
          .map(
            (c) => `<tr>
        <td>${escapeHtml(c.serie || "")}-${escapeHtml(c.numeroFactura || "")}</td>
        <td>${escapeHtml(c.razonSocialCliente || "")}</td>
        <td>${formatMoney(c.montoTotal)}</td>
        <td><strong>${formatMoney(c.saldoPendiente)}</strong></td>
        <td>${formatDate(c.fechaVencimiento)}</td>
        <td><span class="badge ${estadoBadge(c.estado)}">${escapeHtml(c.estado)}</span></td>
        <td>${c.estado !== "Pagado" ? `<button class="btn-primary btn-sm" onclick="registrarPago(${c.idCxc})">Registrar Pago</button>` : ""}</td>
      </tr>`,
          )
          .join("") || '<tr><td colspan="7">Sin cuentas por cobrar</td></tr>'
      }</tbody></table>`;
  } catch (e) {
    showToast(e.message, "error");
  }
}

function registrarPago(id) {
  const monto = prompt("Monto a abonar:");
  if (!monto) return;
  const numOp = prompt("Número de operación bancaria:") || "";
  apiCall(`/cuentas-cobrar/${id}/pago`, {
    method: "POST",
    body: JSON.stringify({
      montoPagado: parseFloat(monto),
      numeroOperacion: numOp,
      metodoPago: "Transferencia",
    }),
  })
    .then(() => {
      showToast("Pago registrado");
      renderCxC();
    })
    .catch((e) => showToast(e.message, "error"));
}

// ============ CxP ============
async function renderCxP() {
  const el = document.getElementById("mainContent");

  el.innerHTML = `
    <div class="page-header">
      <h2>Cuentas por Pagar</h2>
      <button class="btn-primary" onclick="nuevaCuentaPagar()">
        + Nueva Cuenta
      </button>
    </div>

    <div class="card">
      <div class="table-wrapper" id="cxpTable">
        Cargando...
      </div>
    </div>
  `;

  try {
    const data = await apiCall("/cuentas-pagar");

    document.getElementById("cxpTable").innerHTML = `
      <table>
        <thead>
          <tr>
            <th>Proveedor</th>
            <th>Factura</th>
            <th>Total</th>
            <th>Saldo</th>
            <th>Vencimiento</th>
            <th>Estado</th>
            <th>Acciones</th>
          </tr>
        </thead>

        <tbody>
          ${
            data
              .map(
                (c) => `
              <tr>
                <td>${escapeHtml(c.RazonSocialProveedor || "")}</td>

                <td>
                  ${escapeHtml(c.NumeroFacturaProveedor || "-")}
                </td>

                <td>
                  ${formatMoney(c.MontoTotal)}
                </td>

                <td>
                  <strong>${formatMoney(c.SaldoPendiente)}</strong>
                </td>

                <td>
                  ${formatDate(c.FechaVencimiento)}
                </td>

                <td>
                  <span class="badge ${estadoBadge(c.Estado)}">
                    ${escapeHtml(c.Estado || "")}
                  </span>
                </td>

                <td>
                  ${
                    c.Estado !== "Pagado"
                      ? `
                        <button
                          class="btn-primary btn-sm"
                          onclick="registrarPagoCxP(${c.IdCxp})">
                          Registrar Pago
                        </button>
                      `
                      : ""
                  }
                </td>
              </tr>
            `,
              )
              .join("") ||
            `
              <tr>
                <td colspan="7">
                  Sin cuentas por pagar
                </td>
              </tr>
            `
          }
        </tbody>
      </table>
    `;
  } catch (e) {
    showToast(e.message, "error");
  }
}
async function nuevaCuentaPagar() {
  try {
    const proveedores = await apiCall("/proveedores");

    if (proveedores.length === 0) {
      showToast("No hay proveedores registrados", "error");
      return;
    }

    showModal(
      "Nueva Cuenta por Pagar",
      `
        <div class="form-group">
          <label>Proveedor</label>

          <select id="cxpProveedor">
            ${proveedores
              .map(
                (p) => `
                  <option value="${p.IdProveedor}">
                  ${escapeHtml(p.RazonSocial || "")}
                  - ${escapeHtml(p.Ruc || "")}
                  </option>
                `,
              )
              .join("")}
          </select>
        </div>

        <div class="form-group">
          <label>N° Factura del Proveedor</label>

          <input
            type="text"
            id="cxpFactura"
            placeholder="Ej: F001-4587"
          >
        </div>

        <div class="form-group">
          <label>Monto Total</label>

          <input
            type="number"
            id="cxpMonto"
            min="0.01"
            step="0.01"
            placeholder="Ej: 1000.00"
          >
        </div>

        <div class="form-group">
          <label>Fecha de Vencimiento</label>

          <input
            type="date"
            id="cxpVencimiento"
          >
        </div>

        <div style="text-align:right;">
          <button
            class="btn-primary"
            onclick="guardarCuentaPagar()">
            Registrar Cuenta
          </button>
        </div>
      `,
    );
  } catch (e) {
    showToast(e.message, "error");
  }
}
async function guardarCuentaPagar() {
  const idProveedor = parseInt(document.getElementById("cxpProveedor").value);

  const numeroFacturaProveedor = document
    .getElementById("cxpFactura")
    .value.trim();

  const montoTotal = parseFloat(document.getElementById("cxpMonto").value);

  const fechaVencimiento = document.getElementById("cxpVencimiento").value;

  if (!idProveedor) {
    showToast("Seleccione un proveedor", "error");
    return;
  }

  if (!numeroFacturaProveedor) {
    showToast("Ingrese el número de factura", "error");
    return;
  }

  if (!montoTotal || montoTotal <= 0) {
    showToast("Ingrese un monto válido", "error");
    return;
  }

  if (!fechaVencimiento) {
    showToast("Seleccione la fecha de vencimiento", "error");
    return;
  }

  try {
    await apiCall("/cuentas-pagar", {
      method: "POST",
      body: JSON.stringify({
        idProveedor: idProveedor,
        numeroFacturaProveedor: numeroFacturaProveedor,
        montoTotal: montoTotal,
        fechaVencimiento: fechaVencimiento,
      }),
    });

    showToast("Cuenta por pagar registrada correctamente");

    closeModal();

    renderCxP();
  } catch (e) {
    showToast(e.message, "error");
  }
}
async function registrarPagoCxP(id, saldo) {
  const monto = prompt(
    `Saldo pendiente: ${formatMoney(saldo)}\n\nMonto a pagar:`,
  );

  if (!monto) return;

  const montoPago = parseFloat(monto);

  if (isNaN(montoPago) || montoPago <= 0) {
    showToast("Ingrese un monto válido", "error");
    return;
  }

  try {
    await apiCall(`/cuentas-pagar/${id}/pago`, {
      method: "POST",

      body: JSON.stringify({
        monto: montoPago,
      }),
    });

    showToast("Pago registrado correctamente");

    renderCxP();
  } catch (e) {
    showToast(e.message, "error");
  }
}
// ============ SSOMA ============
async function renderSsoma() {
  const el = document.getElementById("mainContent");

  el.innerHTML = `
  <div class="page-header">
  <h2>SSOMA - Calidad y Capacitación</h2>

  <div>
    <button
      class="btn-primary"
      onclick="nuevaFichaTecnica()">
      + Nueva Ficha Técnica
    </button>

    <button
      class="btn-primary"
      onclick="nuevaCapacitacion()">
      + Nueva Capacitación
    </button>
  </div>
</div>

    <div class="stats-grid" id="ssomaStats">
      <div class="card">Cargando...</div>
    </div>

    <div class="card" style="margin-top:20px;">
      <h3>Fichas Técnicas</h3>
      <div class="table-wrapper" id="ssomaFichas">
        Cargando...
      </div>
    </div>

    <div class="card" style="margin-top:20px;">
      <h3>Capacitaciones</h3>
      <div class="table-wrapper" id="ssomaCapacitaciones">
        Cargando...
      </div>
    </div>
  `;

  try {
    const [resumen, fichas, capacitaciones] = await Promise.all([
      apiCall("/ssoma/resumen"),
      apiCall("/ssoma/fichas"),
      apiCall("/ssoma/capacitaciones"),
    ]);

    // RESUMEN
    document.getElementById("ssomaStats").innerHTML = `
      <div class="stat-card">
        <div class="label">Fichas Técnicas</div>
        <div class="value">${resumen.fichas || 0}</div>
        <div class="sub">Documentos técnicos</div>
      </div>

      <div class="stat-card">
        <div class="label">Certificados Calidad</div>
        <div class="value">${resumen.certificados || 0}</div>
        <div class="sub">Certificados registrados</div>
      </div>

      <div class="stat-card">
        <div class="label">Capacitaciones</div>
        <div class="value">${resumen.capacitaciones || 0}</div>
        <div class="sub">Capacitaciones realizadas</div>
      </div>
    `;

    // FICHAS TÉCNICAS
    document.getElementById("ssomaFichas").innerHTML = `
      <table>
        <thead>
          <tr>
            <th>SKU</th>
            <th>Producto</th>
            <th>Documento</th>
          </tr>
        </thead>

        <tbody>
          ${
            fichas.length
              ? fichas
                  .map(
                    (f) => `
                  <tr>
                    <td>${escapeHtml(f.Sku || f.sku || "-")}</td>

                    <td>
                      ${escapeHtml(f.NombreProducto || f.nombreProducto || "-")}
                    </td>

                    <td>
                      Ficha técnica registrada
                    </td>
                  </tr>
                `,
                  )
                  .join("")
              : `
                <tr>
                  <td colspan="3">
                    No hay fichas técnicas registradas
                  </td>
                </tr>
              `
          }
        </tbody>
      </table>
    `;

    // CAPACITACIONES
    document.getElementById("ssomaCapacitaciones").innerHTML = `
      <table>
        <thead>
          <tr>
            <th>Cliente</th>
            <th>Instructor</th>
            <th>Tema</th>
            <th>Fecha</th>
          </tr>
        </thead>

        <tbody>
          ${
            capacitaciones.length
              ? capacitaciones
                  .map(
                    (c) => `
                  <tr>
                    <td>
                      ${escapeHtml(
                        c.RazonSocialCliente || c.razonSocialCliente || "-",
                      )}
                    </td>

                    <td>
                      ${escapeHtml(
                        c.NombreInstructor || c.nombreInstructor || "-",
                      )}
                    </td>

                    <td>
                      ${escapeHtml(c.Tema || c.tema || "-")}
                    </td>

                    <td>
                      ${
                        c.fecha_capacitacion
                          ? formatDate(c.fecha_capacitacion)
                          : "-"
                      }
                    </td>
                  </tr>
                `,
                  )
                  .join("")
              : `
                <tr>
                  <td colspan="4">
                    No hay capacitaciones registradas
                  </td>
                </tr>
              `
          }
        </tbody>
      </table>
    `;
  } catch (e) {
    showToast(e.message, "error");
  }
}
async function nuevaCapacitacion() {
  try {
    const [clientes, usuarios] = await Promise.all([
      apiCall("/clientes"),
      apiCall("/usuarios"),
    ]);

    const opcionesClientes = clientes
      .map(
        (c) =>
          `<option value="${c.idCliente}">
            ${escapeHtml(c.razonSocial)}
          </option>`,
      )
      .join("");

    const opcionesInstructores = usuarios
      .map(
        (u) =>
          `<option value="${u.idUsuario}">
            ${escapeHtml(u.nombres)} ${escapeHtml(u.apellidos || "")}
          </option>`,
      )
      .join("");

    showModal(
      "Nueva Capacitación",
      `
        <div class="form-group">
          <label>Cliente</label>
          <select id="capCliente">
            <option value="">Seleccione un cliente</option>
            ${opcionesClientes}
          </select>
        </div>

        <div class="form-group">
          <label>Instructor</label>
          <select id="capInstructor">
            <option value="">Seleccione un instructor</option>
            ${opcionesInstructores}
          </select>
        </div>

        <div class="form-group">
          <label>Tema de la capacitación</label>
          <input
            type="text"
            id="capTema"
            placeholder="Ej: Uso correcto de EPP"
          >
        </div>

        <div class="form-group">
          <label>Fecha</label>
          <input
            type="date"
            id="capFecha"
          >
        </div>

        <div class="form-group">
          <label>Lugar</label>
          <input
            type="text"
            id="capLugar"
            placeholder="Ej: Planta del cliente"
          >
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>Hora de inicio</label>
            <input
              type="time"
              id="capHoraInicio"
            >
          </div>

          <div class="form-group">
            <label>Hora de fin</label>
            <input
              type="time"
              id="capHoraFin"
            >
          </div>
        </div>

        <div style="text-align:right; margin-top:20px;">
          <button
            class="btn-primary"
            onclick="guardarCapacitacion()">
            Registrar Capacitación
          </button>
        </div>
      `,
    );
  } catch (e) {
    showToast(e.message, "error");
  }
}

async function guardarCapacitacion() {
  const idCliente = parseInt(document.getElementById("capCliente").value);

  const instructor = document.getElementById("capInstructor").value;

  const tema = document.getElementById("capTema").value.trim();

  const fecha = document.getElementById("capFecha").value;

  const lugar = document.getElementById("capLugar").value.trim();

  const horaInicio = document.getElementById("capHoraInicio").value;

  const horaFin = document.getElementById("capHoraFin").value;

  if (!idCliente || !tema || !fecha) {
    showToast("Cliente, tema y fecha son obligatorios", "error");
    return;
  }

  try {
    await apiCall("/ssoma/capacitaciones", {
      method: "POST",

      body: JSON.stringify({
        idCliente: idCliente,

        idUsuarioInstructor: instructor ? parseInt(instructor) : null,

        tema: tema,

        fechaCapacitacion: fecha,

        lugar: lugar,

        horaInicio: horaInicio ? horaInicio + ":00" : null,

        horaFin: horaFin ? horaFin + ":00" : null,
      }),
    });

    closeModal();

    showToast("Capacitación registrada correctamente");

    renderSsoma();
  } catch (e) {
    showToast(e.message, "error");
  }
}
async function nuevaFichaTecnica() {
  try {
    const productos = await apiCall("/productos");

    showModal(
      "Nueva Ficha Técnica",
      `
        <div class="form-group">
          <label>Producto</label>
          <select id="ftProducto">
            ${productos
              .map(
                (p) => `
                  <option value="${p.idProducto}">
                    ${escapeHtml(p.sku)} - ${escapeHtml(p.nombre)}
                  </option>
                `,
              )
              .join("")}
          </select>
        </div>

        <div class="form-group">
          <label>Nombre del archivo</label>
          <input
            id="ftNombre"
            type="text"
            placeholder="Ej: Ficha_Tecnica_Casco_3M.pdf"
          >
        </div>

        <div class="form-group">
          <label>URL / Ruta del documento</label>
          <input
            id="ftUrl"
            type="text"
            placeholder="/documentos/ficha_casco.pdf"
          >
        </div>

        <div class="form-group">
          <label>Versión</label>
          <input
            id="ftVersion"
            type="text"
            value="1.0"
          >
        </div>

        <div style="text-align:right;">
          <button
            class="btn-primary"
            onclick="guardarFichaTecnica()">
            Guardar Ficha
          </button>
        </div>
      `,
    );
  } catch (e) {
    showToast(e.message, "error");
  }
}

async function guardarFichaTecnica() {
  const idProducto = parseInt(document.getElementById("ftProducto").value);

  const nombreArchivo = document.getElementById("ftNombre").value.trim();

  const urlArchivo = document.getElementById("ftUrl").value.trim();

  const version = document.getElementById("ftVersion").value.trim();

  if (!nombreArchivo) {
    showToast("Ingrese el nombre del archivo", "error");
    return;
  }

  try {
    await apiCall("/ssoma/fichas", {
      method: "POST",
      body: JSON.stringify({
        idProducto,
        nombreArchivo,
        urlArchivo,
        version,
      }),
    });

    showToast("Ficha técnica registrada correctamente");

    closeModal();

    renderSsoma();
  } catch (e) {
    showToast(e.message, "error");
  }
}
// ============ USUARIOS ============
async function renderUsuarios() {
  const el = document.getElementById("mainContent");
  el.innerHTML = `
    <div class="page-header"><h2>Gestión de Usuarios y Roles</h2>
      <div class="actions"><button class="btn-primary" onclick="nuevoUsuario()">+ Nuevo Usuario</button></div>
    </div>
    <div class="card"><div class="table-wrapper" id="userTable">Cargando...</div></div>`;
  try {
    const data = await apiCall("/usuarios");
    document.getElementById("userTable").innerHTML = `
      <table><thead><tr><th>Nombre</th><th>Email</th><th>Rol</th><th>Estado</th><th>Acciones</th></tr></thead>
      <tbody>${data
        .map(
          (u) => `<tr>
        <td>${escapeHtml(u.nombres)}</td>
        <td>${escapeHtml(u.email)}</td>
        <td><span class="badge badge-info">${escapeHtml(u.nombreRol)}</span></td>
        <td><span class="badge ${u.activo ? "badge-success" : "badge-danger"}">${u.activo ? "Activo" : "Inactivo"}</span></td>
        <td><button class="btn-secondary btn-sm" onclick="resetPassword(${u.idUsuario})">Resetear Password</button></td>
      </tr>`,
        )
        .join("")}</tbody></table>`;
  } catch (e) {
    showToast(e.message, "error");
  }
}

async function nuevoUsuario() {
  const roles = await apiCall("/usuarios/roles");
  showModal(
    "Nuevo Usuario",
    `
    <div class="form-group"><label>Nombres</label><input id="uNombres"></div>
    <div class="form-group"><label>Email</label><input type="email" id="uEmail"></div>
    <div class="form-group"><label>Password</label><input type="password" id="uPass" value="Security2026!"></div>
    <div class="form-group"><label>Rol</label>
      <select id="uRol">${roles.map((r) => `<option value="${r.idRol}">${escapeHtml(r.nombreRol)}</option>`).join("")}</select>
    </div>
    <div class="form-row">
      <div class="form-group"><label>DNI</label><input id="uDni" maxlength="8"></div>
      <div class="form-group"><label>Teléfono</label><input id="uTel"></div>
    </div>
    <div style="text-align:right;"><button class="btn-primary" onclick="guardarUsuario()">Crear Usuario</button></div>`,
  );
}

async function guardarUsuario() {
  const body = {
    idRol: parseInt(document.getElementById("uRol").value),
    nombres: document.getElementById("uNombres").value,
    email: document.getElementById("uEmail").value,
    password: document.getElementById("uPass").value,
    dni: document.getElementById("uDni").value,
    telefono: document.getElementById("uTel").value,
  };
  try {
    await apiCall("/usuarios", { method: "POST", body: JSON.stringify(body) });
    showToast("Usuario creado");
    closeModal();
    renderUsuarios();
  } catch (e) {
    showToast(e.message, "error");
  }
}

async function resetPassword(id) {
  const pwd = prompt("Nueva contraseña:");
  if (!pwd) return;
  try {
    await apiCall(`/usuarios/${id}/password`, {
      method: "PUT",
      body: JSON.stringify(pwd),
    });
    showToast("Password actualizado");
  } catch (e) {
    showToast(e.message, "error");
  }
}

// ============ COMISIONES ============
async function renderComisiones() {
  const el = document.getElementById("mainContent");

  el.innerHTML = `
    <div class="page-header">
      <h2>Liquidación de Comisiones</h2>
    </div>

    <div class="card">
      <div class="table-wrapper" id="comTable">
        Cargando...
      </div>
    </div>
  `;

  try {
    const data = await apiCall("/comisiones");

    document.getElementById("comTable").innerHTML = `
      <table>
        <thead>
          <tr>
            <th>Vendedor</th>
            <th>Periodo</th>
            <th>Monto Cobrado</th>
            <th>% Comisión</th>
            <th>Comisión Neta</th>
            <th>Estado</th>
          </tr>
        </thead>

        <tbody>
          ${
            data.length
              ? data
                  .map(
                    (c) => `
                <tr>
                  <td>
                    ${escapeHtml(c.NombreVendedor || "-")}
                  </td>

                  <td>
                    ${c.periodo_mes}/${c.periodo_anio}
                  </td>

                  <td>
                    ${formatMoney(c.monto_facturado_cobrado)}
                  </td>

                  <td>
                    ${c.porcentaje_comision}%
                  </td>

                  <td>
                    <strong>
                      ${formatMoney(c.comision_neta)}
                    </strong>
                  </td>

                  <td>
                    <span class="badge ${estadoBadge(c.estado)}">
                      ${escapeHtml(c.estado || "")}
                    </span>
                  </td>
                </tr>
              `,
                  )
                  .join("")
              : `
                <tr>
                  <td colspan="6">
                    Sin liquidaciones
                  </td>
                </tr>
              `
          }
        </tbody>
      </table>
    `;
  } catch (e) {
    showToast(e.message, "error");
  }
}

// ============ INIT ============
window.addEventListener("DOMContentLoaded", () => {
  if (TOKEN && USER) initApp();
});

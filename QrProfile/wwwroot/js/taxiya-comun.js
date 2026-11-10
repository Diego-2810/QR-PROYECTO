// Código compartido entre la tarjeta pública (index.html) y el panel del taxista (administracion.html).
(function () {
    'use strict';

    // ---------- Catálogo de servicios ----------
    // Se guarda en la base con el texto en español ("es"). La tarjeta lo muestra traducido.
    // "alias" son otras formas en que un taxista puede haberlo escrito antes.
    // Importante: ningún texto puede llevar coma, porque la coma separa servicios.
    const SERVICIOS = [
        { icon: 'bi-person-check', es: 'Traslados particulares', en: 'Private transfers', pt: 'Transfers particulares', alias: ['viajes particulares'] },
        { icon: 'bi-airplane', es: 'Aeropuertos', en: 'Airport transfers', pt: 'Transfers para aeroportos', alias: ['aeropuerto', 'traslados al aeropuerto', 'traslados a aeropuertos'] },
        { icon: 'bi-calendar-check', es: 'Viajes programados', en: 'Scheduled rides', pt: 'Viagens agendadas', alias: ['reservas', 'viajes con reserva'] },
        { icon: 'bi-signpost-split', es: 'Viajes de larga distancia', en: 'Long-distance trips', pt: 'Viagens de longa distância', alias: ['viajes largos', 'larga distancia'] },
        { icon: 'bi-camera', es: 'Paseos turísticos', en: 'City tours', pt: 'Passeios turísticos', alias: ['servicios de tour', 'tour', 'tours', 'city tour'] },
        { icon: 'bi-briefcase', es: 'Servicio para empresas', en: 'Corporate service', pt: 'Serviço para empresas', alias: ['empresas', 'traslados laborales'] },
        { icon: 'bi-clock', es: 'Servicio 24 horas', en: '24/7 service', pt: 'Serviço 24 horas', alias: ['24 horas', '24hs', '24 hs'] },
        { icon: 'bi-suitcase2', es: 'Equipaje grande', en: 'Large luggage', pt: 'Bagagem grande', alias: ['baul grande', 'valijas'] },
        { icon: 'bi-snow', es: 'Aire acondicionado', en: 'Air conditioning', pt: 'Ar-condicionado', alias: ['aire'] },
        { icon: 'bi-heart', es: 'Acepta mascotas', en: 'Pets welcome', pt: 'Aceita animais de estimação', alias: ['mascotas', 'pet friendly'] },
        { icon: 'bi-credit-card', es: 'Pago con tarjeta', en: 'Card payment accepted', pt: 'Aceita cartão', alias: ['tarjeta', 'tarjeta de credito', 'tarjeta de debito'] },
        { icon: 'bi-qr-code-scan', es: 'Pago con Mercado Pago o QR', en: 'Mercado Pago / QR payment', pt: 'Pagamento com Mercado Pago ou QR', alias: ['mercado pago', 'pago con qr', 'qr'] },
        { icon: 'bi-currency-dollar', es: 'Acepta dólares', en: 'US dollars accepted', pt: 'Aceita dólares', alias: ['dolares', 'usd'] },
        { icon: 'bi-translate', es: 'Habla inglés', en: 'English spoken', pt: 'Fala inglês', alias: ['ingles', 'english'] },
        { icon: 'bi-translate', es: 'Habla portugués', en: 'Portuguese spoken', pt: 'Fala português', alias: ['portugues', 'português'] }
    ];

    // Minúsculas, sin tildes y sin espacios repetidos, para comparar sin importar cómo se escribió.
    function normalizarTexto(valor) {
        return String(valor ?? '').normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase().replace(/\s+/g, ' ').trim();
    }

    const indice = new Map();
    for (const servicio of SERVICIOS) {
        for (const texto of [servicio.es, servicio.en, servicio.pt, ...(servicio.alias || [])]) {
            indice.set(normalizarTexto(texto), servicio);
        }
    }

    /** Devuelve el servicio del catálogo que coincide con el texto, o null si es un servicio propio. */
    function buscarServicio(texto) {
        return indice.get(normalizarTexto(texto)) || null;
    }

    /** Separa el campo Servicios en una lista (por coma, punto y coma o salto de línea). */
    function separarServicios(texto) {
        return String(texto ?? '').split(/[,;\n]/).map(item => item.trim()).filter(Boolean);
    }

    // ---------- Formato del teléfono ----------
    // Características de Argentina de 2 y 3 dígitos; el resto son de 4.
    const AREAS_AR_3 = new Set(['220', '221', '223', '230', '236', '237', '249', '260', '261', '263', '264', '266', '280', '291', '297', '298', '299',
        '336', '341', '342', '343', '345', '348', '351', '353', '358', '362', '364', '370', '376', '379', '380', '381', '383', '385', '387', '388']);

    function formatoArgentina(nacional) {
        // nacional: 10 dígitos = característica + número (sin 0 ni 15).
        if (nacional.length !== 10) return null;
        const largoArea = nacional.startsWith('11') ? 2 : AREAS_AR_3.has(nacional.slice(0, 3)) ? 3 : 4;
        const area = nacional.slice(0, largoArea);
        const numero = nacional.slice(largoArea);
        const corte = numero.length - 4;
        return `${area} ${numero.slice(0, corte)}-${numero.slice(corte)}`;
    }

    /**
     * Muestra un teléfono E.164 (+5491131788229) de forma legible (+54 9 11 3178-8229).
     * Si no reconoce el país, lo devuelve tal cual. Para links (wa.me, tel:) se usa siempre el original.
     */
    function formatearTelefono(e164) {
        const valor = String(e164 ?? '').trim();
        if (!/^\+\d{8,15}$/.test(valor)) return valor;
        const d = valor.slice(1);

        if (d.startsWith('549')) { const r = formatoArgentina(d.slice(3)); if (r) return `+54 9 ${r}`; }
        if (d.startsWith('54')) { const r = formatoArgentina(d.slice(2)); if (r) return `+54 ${r}`; }
        if (d.startsWith('55') && (d.length === 12 || d.length === 13)) {
            const area = d.slice(2, 4), numero = d.slice(4), corte = numero.length - 4;
            return `+55 ${area} ${numero.slice(0, corte)}-${numero.slice(corte)}`;
        }
        if (d.startsWith('598') && d.length === 11) return `+598 ${d.slice(3, 5)} ${d.slice(5, 8)} ${d.slice(8)}`;
        if (d.startsWith('569') && d.length === 11) return `+56 9 ${d.slice(3, 7)} ${d.slice(7)}`;
        if (d.startsWith('1') && d.length === 11) return `+1 ${d.slice(1, 4)} ${d.slice(4, 7)} ${d.slice(7)}`;
        return valor;
    }

    // ---------- Fotos y videos de los viajes ----------
    // Cloudinary transforma al vuelo según la URL: se inserta la transformación después de "/upload/".
    function transformarCloudinary(url, transformacion, extension) {
        const valor = String(url ?? '');
        const marca = valor.indexOf('/upload/');
        if (marca < 0) return valor;
        let resultado = valor.slice(0, marca + 8) + transformacion + '/' + valor.slice(marca + 8);
        if (extension) resultado = resultado.replace(/\.[a-z0-9]+$/i, '.' + extension);
        return resultado;
    }

    function idYouTube(url) {
        const coincidencia = /[?&]v=([A-Za-z0-9_-]{11})/.exec(String(url ?? ''));
        return coincidencia ? coincidencia[1] : null;
    }

    /** Miniatura recortada para grillas. Devuelve '' si el medio no tiene imagen (links de Instagram, etc.). */
    function miniaturaMedio(medio, ancho = 600, alto = 450) {
        if (medio.tipo === 'imagen') return transformarCloudinary(medio.url, `c_fill,w_${ancho},h_${alto},f_auto,q_auto`, 'jpg');
        if (medio.tipo === 'video') return transformarCloudinary(medio.url, `so_0,c_fill,w_${ancho},h_${alto},q_auto`, 'jpg');
        if (medio.tipo === 'youtube') { const id = idYouTube(medio.url); return id ? `https://i.ytimg.com/vi/${id}/hqdefault.jpg` : ''; }
        return '';
    }

    /** Foto en tamaño de pantalla, en el formato más liviano que acepte el navegador (convierte HEIC). */
    function urlImagenGrande(url) { return transformarCloudinary(url, 'c_limit,w_1600,h_1600,f_auto,q_auto', 'jpg'); }

    /** Video comprimido en MP4, compatible con iPhone y Android aunque se haya subido en .mov. */
    function urlVideo(url) { return transformarCloudinary(url, 'c_limit,w_1280,h_1280,q_auto,vc_auto', 'mp4'); }

    function urlEmbedYouTube(url) { const id = idYouTube(url); return id ? `https://www.youtube-nocookie.com/embed/${id}?rel=0` : ''; }

    /** Nombre del sitio de un link ("Instagram", "TikTok", ...). */
    function sitioEnlace(url) {
        try {
            const host = new URL(url).hostname.replace(/^(www|m|vm|web)\./, '');
            return { 'instagram.com': 'Instagram', 'tiktok.com': 'TikTok', 'facebook.com': 'Facebook', 'fb.watch': 'Facebook', 'vimeo.com': 'Vimeo', 'youtube.com': 'YouTube' }[host] || host;
        } catch { return ''; }
    }

    window.TaxiYa = Object.freeze({
        SERVICIOS, normalizarTexto, buscarServicio, separarServicios, formatearTelefono,
        miniaturaMedio, urlImagenGrande, urlVideo, urlEmbedYouTube, idYouTube, sitioEnlace
    });
})();

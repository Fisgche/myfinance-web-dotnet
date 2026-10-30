// Dashboard da home (Views/Home/Index.cshtml): grafico de evolucao no mes e dicas das barras.
// Os dados do grafico chegam em <script type="application/json" id="dados-evolucao">.
// Textos que podem vir do usuario (nomes de categoria) sao sempre inseridos com textContent.
(() => {
    'use strict';

    const moeda = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });
    const moedaCompacta = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL', notation: 'compact', maximumFractionDigits: 1 });
    const diaPorExtenso = new Intl.DateTimeFormat('pt-BR', { day: 'numeric', month: 'long' });
    const diaCurto = new Intl.DateTimeFormat('pt-BR', { day: '2-digit', month: '2-digit' });
    const SVG = 'http://www.w3.org/2000/svg';

    function criarSvg(nome, atributos, pai) {
        const elemento = document.createElementNS(SVG, nome);
        for (const [chave, valor] of Object.entries(atributos)) {
            elemento.setAttribute(chave, valor);
        }
        if (pai) {
            pai.appendChild(elemento);
        }
        return elemento;
    }

    function criarTexto(conteudo, atributos, pai) {
        const elemento = criarSvg('text', atributos, pai);
        elemento.textContent = conteudo;
        return elemento;
    }

    // Passo "limpo" para o eixo Y: 1, 2, 2,5 ou 5 x 10^n
    function passoLimpo(maximo, divisoes) {
        const bruto = maximo / divisoes;
        const potencia = Math.pow(10, Math.floor(Math.log10(bruto)));
        const fracao = bruto / potencia;
        const passo = fracao <= 1 ? 1 : fracao <= 2 ? 2 : fracao <= 2.5 ? 2.5 : fracao <= 5 ? 5 : 10;
        return passo * potencia;
    }

    // Dica (tooltip) posicionada dentro de "container", que precisa ter position: relative.
    // Cada linha: chave de cor da serie, valor (forte) e nome (secundario).
    // posicao "lado": ao lado do ponto (grafico de linhas, para nao cobrir a mira);
    // posicao "acima": centralizada acima do ponto (barras, para nao cobrir o rotulo da barra).
    function criarDica(container) {
        const dica = document.createElement('div');
        dica.className = 'dashboard-dica';
        dica.hidden = true;
        dica.setAttribute('aria-hidden', 'true');
        container.appendChild(dica);

        function mostrar(titulo, linhas, x, y, posicao = 'lado') {
            dica.replaceChildren();

            const cabecalho = document.createElement('div');
            cabecalho.className = 'dica-titulo';
            cabecalho.textContent = titulo;
            dica.appendChild(cabecalho);

            for (const linha of linhas) {
                const item = document.createElement('div');
                item.className = 'dica-linha';

                const chave = document.createElement('span');
                chave.className = 'chave-linha chave-' + linha.chave;
                item.appendChild(chave);

                const valor = document.createElement('span');
                valor.className = 'dica-valor';
                valor.textContent = linha.valor;
                item.appendChild(valor);

                const nome = document.createElement('span');
                nome.className = 'dica-nome';
                nome.textContent = linha.nome;
                item.appendChild(nome);

                dica.appendChild(item);
            }

            dica.hidden = false;

            const largura = dica.offsetWidth;
            const altura = dica.offsetHeight;
            let esquerda;
            let topo;
            if (posicao === 'acima') {
                esquerda = Math.max(0, Math.min(x - largura / 2, container.clientWidth - largura));
                topo = y - altura - 12;
                if (topo < 0) {
                    topo = y + 12;
                }
            } else {
                // A direita do ponto; perto da borda direita, vira para a esquerda
                esquerda = x + 16;
                if (esquerda + largura > container.clientWidth) {
                    esquerda = Math.max(0, x - 16 - largura);
                }
                topo = Math.max(0, Math.min(y - altura / 2, container.clientHeight - altura));
            }
            dica.style.left = esquerda + 'px';
            dica.style.top = topo + 'px';
        }

        function esconder() {
            dica.hidden = true;
        }

        return { mostrar, esconder };
    }

    // Grafico de linhas com os valores acumulados no mes (uma linha por tipo com movimento)
    function graficoEvolucao(container, dados, anuncio) {
        const dias = new Date(dados.ano, dados.mes, 0).getDate();
        const series = dados.series;
        const dica = criarDica(container);
        const dataDoDia = dia => new Date(dados.ano, dados.mes - 1, dia);
        let geometria = null;
        let diaSelecionado = null;

        function desenhar() {
            const largura = container.clientWidth;
            if (largura === 0) {
                return;
            }

            const maximo = Math.max(0, ...series.flatMap(serie => serie.valores));
            const passo = passoLimpo(maximo || 1, 4);
            const quantidadeMarcasY = Math.max(1, Math.ceil(maximo / passo));
            const topoEscala = quantidadeMarcasY * passo;

            container.querySelector('svg')?.remove();
            const raiz = criarSvg('svg', { 'aria-hidden': 'true' });
            container.prepend(raiz);

            // A margem esquerda acompanha o maior rotulo do eixo Y
            const rotulosY = [];
            for (let i = 0; i <= quantidadeMarcasY; i++) {
                rotulosY.push(criarTexto(moedaCompacta.format(i * passo), { 'text-anchor': 'end', 'dominant-baseline': 'middle' }, raiz));
            }
            const larguraRotulosY = Math.max(...rotulosY.map(rotulo => rotulo.getComputedTextLength()));

            const comRotulosFinais = largura >= 480;
            const margem = { topo: 20, direita: comRotulosFinais ? 104 : 16, base: 28, esquerda: Math.ceil(larguraRotulosY) + 12 };
            const alturaPlot = 220;
            const larguraPlot = largura - margem.esquerda - margem.direita;
            raiz.setAttribute('width', largura);
            raiz.setAttribute('height', margem.topo + alturaPlot + margem.base);

            const x = dia => margem.esquerda + (dias === 1 ? 0 : (dia - 1) / (dias - 1) * larguraPlot);
            const y = valor => margem.topo + alturaPlot - (valor / topoEscala) * alturaPlot;
            const nitido = valor => Math.round(valor) + 0.5; // linhas de 1px sem borrar

            // Grade (linhas finas e solidas) e linha de base no zero
            rotulosY.forEach((rotulo, i) => {
                const yy = nitido(y(i * passo));
                const linha = criarSvg('line', { x1: margem.esquerda, x2: margem.esquerda + larguraPlot, y1: yy, y2: yy, class: i === 0 ? 'linha-base' : 'grade' });
                raiz.insertBefore(linha, raiz.firstChild);
                rotulo.setAttribute('x', margem.esquerda - 8);
                rotulo.setAttribute('y', yy);
            });

            // Eixo X: dias de referencia, sem duas marcas coladas no fim do mes
            const marcasX = (larguraPlot < 360 ? [1, 10, 20] : [1, 5, 10, 15, 20, 25]).filter(dia => dias - dia >= 4);
            marcasX.push(dias);
            for (const dia of marcasX) {
                criarTexto(diaCurto.format(dataDoDia(dia)), {
                    x: x(dia),
                    y: margem.topo + alturaPlot + 18,
                    'text-anchor': dia === 1 ? 'start' : dia === dias ? 'end' : 'middle'
                }, raiz);
            }

            // "Hoje", quando o mes exibido e o atual
            if (dados.hoje) {
                const xHoje = nitido(x(dados.hoje));
                criarSvg('line', { x1: xHoje, x2: xHoje, y1: margem.topo, y2: margem.topo + alturaPlot, class: 'marcador-hoje' }, raiz);
                const ancora = dados.hoje <= 2 ? 'start' : dados.hoje >= dias - 1 ? 'end' : 'middle';
                criarTexto('Hoje', { x: xHoje, y: margem.topo - 6, 'text-anchor': ancora, class: 'rotulo-hoje' }, raiz);
            }

            // Linhas (2px) com o ponto final
            const rotulosFinais = [];
            for (const serie of series) {
                const caminho = serie.valores
                    .map((valor, i) => (i === 0 ? 'M' : 'L') + x(i + 1).toFixed(1) + ',' + y(valor).toFixed(1))
                    .join('');
                criarSvg('path', { d: caminho, class: 'linha' }, raiz).style.stroke = `var(--serie-${serie.chave})`;

                const ultimo = serie.valores[serie.valores.length - 1];
                criarSvg('circle', { cx: x(dias), cy: y(ultimo), r: 5, class: 'marcador' }, raiz).style.fill = `var(--serie-${serie.chave})`;
                rotulosFinais.push({ serie, valor: ultimo, y: y(ultimo) });
            }

            // Rotulo no fim de cada linha (nome e total). Se dois ficariam colados, nao sao
            // empilhados: ficam so a legenda e a dica.
            rotulosFinais.sort((a, b) => a.y - b.y);
            const colidem = rotulosFinais.some((rotulo, i) => i > 0 && rotulo.y - rotulosFinais[i - 1].y < 30);
            if (comRotulosFinais && !colidem) {
                for (const rotulo of rotulosFinais) {
                    const texto = criarSvg('text', { class: 'rotulo-final' }, raiz);
                    criarSvg('tspan', { x: x(dias) + 12, y: rotulo.y - 3 }, texto).textContent = rotulo.serie.nome;
                    criarSvg('tspan', { x: x(dias) + 12, y: rotulo.y + 12, class: 'rotulo-final-valor' }, texto).textContent = moedaCompacta.format(rotulo.valor);
                }
            }

            // Mira: linha vertical no dia mais proximo do ponteiro, com o ponto de cada serie
            const mira = criarSvg('g', { visibility: 'hidden' }, raiz);
            const linhaMira = criarSvg('line', { y1: margem.topo, y2: margem.topo + alturaPlot, class: 'linha-mira' }, mira);
            const pontos = series.map(serie => {
                const ponto = criarSvg('circle', { r: 5, class: 'marcador' }, mira);
                ponto.style.fill = `var(--serie-${serie.chave})`;
                return ponto;
            });

            geometria = { x, y, margem, larguraPlot, mira, linhaMira, pontos };

            if (diaSelecionado) {
                mostrarDia(diaSelecionado);
            }
        }

        function mostrarDia(dia, ponteiroY) {
            diaSelecionado = dia;
            const { x, y, margem, mira, linhaMira, pontos } = geometria;
            const xMira = Math.round(x(dia)) + 0.5;
            linhaMira.setAttribute('x1', xMira);
            linhaMira.setAttribute('x2', xMira);
            series.forEach((serie, i) => {
                pontos[i].setAttribute('cx', x(dia));
                pontos[i].setAttribute('cy', y(serie.valores[dia - 1]));
            });
            mira.setAttribute('visibility', 'visible');

            dica.mostrar(
                diaPorExtenso.format(dataDoDia(dia)),
                series.map(serie => ({ chave: serie.chave, valor: moeda.format(serie.valores[dia - 1]), nome: serie.nome })),
                x(dia),
                ponteiroY ?? margem.topo + 48);
        }

        function esconder() {
            diaSelecionado = null;
            if (geometria) {
                geometria.mira.setAttribute('visibility', 'hidden');
            }
            dica.esconder();
        }

        // Pelo teclado, o leitor de tela recebe os mesmos valores da dica
        function anunciar() {
            if (!anuncio || !diaSelecionado) {
                return;
            }
            anuncio.textContent = diaPorExtenso.format(dataDoDia(diaSelecionado)) + ': ' +
                series.map(serie => serie.nome + ' ' + moeda.format(serie.valores[diaSelecionado - 1])).join(', ');
        }

        container.addEventListener('pointermove', evento => {
            if (!geometria) {
                return;
            }
            const caixa = container.getBoundingClientRect();
            const proporcao = (evento.clientX - caixa.left - geometria.margem.esquerda) / geometria.larguraPlot;
            const dia = Math.min(dias, Math.max(1, Math.round(proporcao * (dias - 1)) + 1));
            mostrarDia(dia, evento.clientY - caixa.top);
        });

        container.addEventListener('pointerleave', () => {
            if (!container.matches(':focus-visible')) {
                esconder();
            }
        });

        container.addEventListener('focus', () => {
            if (container.matches(':focus-visible')) {
                mostrarDia(diaSelecionado ?? dados.hoje ?? dias);
                anunciar();
            }
        });

        container.addEventListener('blur', esconder);

        container.addEventListener('keydown', evento => {
            let dia = diaSelecionado ?? dados.hoje ?? dias;
            switch (evento.key) {
                case 'ArrowLeft': dia = Math.max(1, dia - 1); break;
                case 'ArrowRight': dia = Math.min(dias, dia + 1); break;
                case 'Home': dia = 1; break;
                case 'End': dia = dias; break;
                case 'Escape': esconder(); return;
                default: return;
            }
            evento.preventDefault();
            mostrarDia(dia);
            anunciar();
        });

        desenhar();

        // Redesenha na largura real do cartao, para o texto continuar em 12px
        let redesenhoAgendado = false;
        new ResizeObserver(() => {
            if (redesenhoAgendado) {
                return;
            }
            redesenhoAgendado = true;
            requestAnimationFrame(() => {
                redesenhoAgendado = false;
                desenhar();
            });
        }).observe(container);
    }

    // Dica de cada barra de "Despesas por categoria" (valor e participacao no total)
    function dicasDasBarras(lista) {
        const cartao = lista.closest('.dashboard-cartao');
        const dica = criarDica(cartao);

        for (const barra of lista.querySelectorAll('.barra')) {
            const mostrar = (x, y) => dica.mostrar(barra.dataset.dicaTitulo, [{
                chave: 'despesas',
                valor: barra.dataset.dicaValor,
                nome: barra.dataset.dicaDetalhe
            }], x, y, 'acima');

            barra.addEventListener('pointermove', evento => {
                const caixa = cartao.getBoundingClientRect();
                mostrar(evento.clientX - caixa.left, evento.clientY - caixa.top);
            });
            barra.addEventListener('pointerleave', dica.esconder);
            barra.addEventListener('focus', () => {
                const caixa = cartao.getBoundingClientRect();
                const preenchimento = barra.querySelector('.barra-preenchimento').getBoundingClientRect();
                mostrar(preenchimento.right - caixa.left, preenchimento.top - caixa.top);
            });
            barra.addEventListener('blur', dica.esconder);
        }
    }

    const fonte = document.getElementById('dados-evolucao');
    const grafico = document.getElementById('grafico-evolucao');
    if (fonte && grafico) {
        graficoEvolucao(grafico, JSON.parse(fonte.textContent), document.getElementById('anuncio-grafico'));
    }

    document.querySelectorAll('.dashboard .barras').forEach(dicasDasBarras);
})();

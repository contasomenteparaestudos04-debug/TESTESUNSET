# Funções auxiliares para montar a documentação final a partir da documentação antiga.
import copy
import docx
from docx.shared import Pt, Cm, Emu
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml.ns import qn
from docx.oxml import OxmlElement

RAIZ = '/sessions/PLACEHOLDER'

def abrir(caminho):
    return docx.Document(caminho)

class Montador:
    def __init__(self, doc):
        self.doc = doc
        ps = doc.paragraphs
        # parágrafos modelo (copiados do documento antigo para manter a formatação)
        self.m_texto = copy.deepcopy(ps[28]._p)
        self.m_h2 = copy.deepcopy(ps[27]._p)
        self.m_h3 = copy.deepcopy(ps[29]._p)
        self.m_h4 = copy.deepcopy(ps[35]._p)
        legenda = next(p for p in ps if p.text.startswith('Figura') or p.text.startswith('Fonte'))
        self.m_legenda = copy.deepcopy(legenda._p)
        self.m_tabela = copy.deepcopy(doc.tables[0]._tbl)
        self.ancora = None  # elemento antes do qual tudo é inserido

    def _novo(self, modelo, texto):
        p = copy.deepcopy(modelo)
        runs = p.findall(qn('w:r'))
        for r in runs[1:]:
            p.remove(r)
        for extra in p.findall(qn('w:hyperlink')) + p.findall(qn('w:bookmarkStart')) + p.findall(qn('w:bookmarkEnd')):
            p.remove(extra)
        r = p.find(qn('w:r'))
        for t in r.findall(qn('w:t')) + r.findall(qn('w:br')) + r.findall(qn('w:tab')):
            r.remove(t)
        partes = texto.split('\n')
        for i, parte in enumerate(partes):
            if i > 0:
                r.append(OxmlElement('w:br'))
            t = OxmlElement('w:t')
            t.text = parte
            t.set('{http://www.w3.org/XML/1998/namespace}space', 'preserve')
            r.append(t)
        self.ancora.addprevious(p)
        return docx.text.paragraph.Paragraph(p, self.doc._body)

    def h2(self, t): return self._novo(self.m_h2, t)
    def h3(self, t): return self._novo(self.m_h3, t)
    def h4(self, t): return self._novo(self.m_h4, t)
    def texto(self, t): return self._novo(self.m_texto, t)

    def lista(self, itens):
        return self._novo(self.m_texto, '\n'.join('- ' + i for i in itens))

    def legenda(self, t):
        p = self._novo(self.m_legenda, t)
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        return p

    def codigo(self, trecho, legenda=None):
        # bloco de código: fonte Courier, fundo cinza claro, sem recuo
        for linha in trecho.strip('\n').split('\n'):
            p = self._novo(self.m_texto, linha if linha else ' ')
            pf = p.paragraph_format
            pf.first_line_indent = Cm(0)
            pf.left_indent = Cm(0.5)
            pf.line_spacing = 1.0
            pf.space_after = Pt(0)
            pf.space_before = Pt(0)
            p.alignment = WD_ALIGN_PARAGRAPH.LEFT
            for r in p.runs:
                r.font.name = 'Courier New'
                r.font.size = Pt(8)
                rpr = r._r.get_or_add_rPr()
                rf = rpr.find(qn('w:rFonts'))
                rf.set(qn('w:eastAsia'), 'Courier New'); rf.set(qn('w:cs'), 'Courier New')
            ppr = p._p.get_or_add_pPr()
            shd = OxmlElement('w:shd')
            shd.set(qn('w:val'), 'clear'); shd.set(qn('w:color'), 'auto'); shd.set(qn('w:fill'), 'F2F2F2')
            ppr.append(shd)
        if legenda:
            self.legenda(legenda)

    def imagem(self, caminho, largura_cm, legenda):
        p = self._novo(self.m_texto, '')
        p.paragraph_format.first_line_indent = Cm(0)
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.paragraph_format.keep_with_next = True
        p.runs[0].add_picture(caminho, width=Cm(largura_cm))
        self.legenda(legenda)

    def marcador(self, t):
        # lembrete para o grupo (print que ainda precisa ser colocado)
        p = self._novo(self.m_texto, t)
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.paragraph_format.first_line_indent = Cm(0)
        for r in p.runs:
            r.font.bold = True
            r.font.color.rgb = docx.shared.RGBColor(0xC0, 0x00, 0x00)
        return p

    def tabela(self, cabecalho, linhas, legenda=None):
        tbl = copy.deepcopy(self.m_tabela)
        trs = tbl.findall(qn('w:tr'))
        cab, dado = trs[0], trs[1]
        for tr in trs[1:]:
            tbl.remove(tr)
        def preencher(tr, valores):
            tcs = tr.findall(qn('w:tc'))
            for tc, v in zip(tcs, valores):
                ps = tc.findall(qn('w:p'))
                for extra in ps[1:]:
                    tc.remove(extra)
                p = ps[0]
                rs = p.findall(qn('w:r'))
                for r in rs[1:]:
                    p.remove(r)
                r = rs[0]
                for t in r.findall(qn('w:t')) + r.findall(qn('w:br')):
                    r.remove(t)
                t = OxmlElement('w:t'); t.text = v
                t.set('{http://www.w3.org/XML/1998/namespace}space', 'preserve')
                r.append(t)
        preencher(cab, cabecalho)
        for lin in linhas:
            nova = copy.deepcopy(dado)
            preencher(nova, lin)
            tbl.append(nova)
        self.ancora.addprevious(tbl)
        # parágrafo vazio depois da tabela (o Word exige algo entre duas tabelas)
        if legenda:
            self.legenda(legenda)
        else:
            self._novo(self.m_texto, ' ')

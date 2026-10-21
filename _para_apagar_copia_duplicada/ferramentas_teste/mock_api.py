# Api de mentira SÓ para testar o app Flutter no navegador (não faz parte da entrega).
import json, datetime
from http.server import BaseHTTPRequestHandler, HTTPServer
estado = {"ags": [], "pags": {}, "prox": 1}
quadras = [{"id":1,"nomeQuadra":"Arena Sao Joao 1","precoHora":150.0,"categoria":"Volei","endereco":"Avenida Joao Fiusa, 100 - Jardim Botanico, Ribeirao Preto/SP"},
           {"id":2,"nomeQuadra":"Beach Club Sol","precoHora":90.0,"categoria":"Beach Tennis","endereco":"Rua A, 5 - Centro, Ribeirao Preto/SP"}]
class H(BaseHTTPRequestHandler):
    def _enviar(self, codigo, corpo=None):
        self.send_response(codigo)
        self.send_header("Access-Control-Allow-Origin","*"); self.send_header("Access-Control-Allow-Headers","*"); self.send_header("Access-Control-Allow-Methods","*")
        self.send_header("Content-Type","application/json")
        self.end_headers()
        if corpo is not None: self.wfile.write(json.dumps(corpo).encode())
    def do_OPTIONS(self): self._enviar(204)
    def corpo(self):
        n=int(self.headers.get("Content-Length",0)); return json.loads(self.rfile.read(n) or b"{}")
    def do_GET(self):
        p=self.path
        if p.startswith("/api/quadras/") and "/horarios" in p:
            qid=int(p.split("/")[3].split("?")[0]); dia=p.split("data=")[1]
            hs=[]
            for h in range(8,22):
                oc=any(a["quadraId"]==qid and a["data"]==dia and a["horaInicio"]==f"{h:02d}:00" and a["status"]!="Cancelado" for a in estado["ags"])
                hs.append({"inicio":f"{h:02d}:00","fim":f"{h+1:02d}:00","ocupado":oc,"passado":False,"agendamentoId":None,"cliente":None})
            return self._enviar(200,{"quadra":quadras[qid-1],"data":dia,"horarios":hs})
        if p.startswith("/api/quadras"): return self._enviar(200,quadras)
        if p.startswith("/api/agendamentos"): 
            return self._enviar(200,[{**a,"podeCancelar":a["status"]!="Cancelado","podePagar":a["statusPagamento"]=="Pendente" and a["status"]!="Cancelado"} for a in estado["ags"]])
        if "/pix" in p:
            pid=int(p.split("/")[3]); pg=estado["pags"][pid]
            return self._enviar(200,{"pagamentoId":pid,"agendamentoId":pid,"quadra":"","data":"","horaInicio":"","horaFim":"","valor":pg["valor"],"codigo":f"SUNSET-PIX-SIMULADO-{pid:06d}-{pg['valor']:.2f}","statusPagamento":pg["status"],"segundosRestantes":295 if pg["status"]=="Pendente" else 0})
        self._enviar(404,{"erro":"nada"})
    def do_POST(self):
        p=self.path; b=self.corpo()
        if p.endswith("/usuarios/login"):
            if b.get("senha")=="Senha1234": return self._enviar(200,{"id":3,"nomeSobrenome":"Bruno Oliveira Santos","email":b["email"],"permissao":"Cliente"})
            return self._enviar(401,{"erro":"Email ou senha incorretos"})
        if p.endswith("/agendamentos/reservar"):
            q=quadras[b["quadraId"]-1]; hi=b["horaInicio"][:5]
            if any(a["quadraId"]==b["quadraId"] and a["data"]==b["data"] and a["horaInicio"]==hi and a["status"]!="Cancelado" for a in estado["ags"]):
                return self._enviar(400,{"erro":"Erro: Esta quadra já possui um agendamento para o horário selecionado."})
            i=estado["prox"]; estado["prox"]+=1
            h=int(hi[:2]); estado["pags"][i]={"valor":q["precoHora"],"status":"Pendente"}
            estado["ags"].append({"id":i,"quadraId":b["quadraId"],"quadra":q["nomeQuadra"],"esporte":q["categoria"],"cliente":"","data":b["data"],"horaInicio":hi,"horaFim":f"{h+1:02d}:00","status":"Pendente","pagamentoId":i,"valor":q["precoHora"],"statusPagamento":"Pendente"})
            return self._enviar(201,{"agendamentoId":i,"pagamentoId":i})
        if p.endswith("/cancelar"):
            i=int(p.split("/")[3])
            for a in estado["ags"]:
                if a["id"]==i: a["status"]="Cancelado"; a["statusPagamento"]="Cancelado"; estado["pags"][i]["status"]="Cancelado"
            return self._enviar(204)
        if p.endswith("/simular"):
            i=int(p.split("/")[3]); estado["pags"][i]["status"]="Aprovado"
            for a in estado["ags"]:
                if a["id"]==i: a["status"]="Em andamento"; a["statusPagamento"]="Aprovado"
            return self._enviar(204)
        self._enviar(404,{"erro":"nada"})
HTTPServer(("0.0.0.0",5218),H).serve_forever()

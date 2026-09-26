package com.example.amfootball.competicao

/** Textos e regras simples partilhados pelos ecrãs de competição (sem dependências do Android). */
object Textos {
    val POSICOES = listOf("Avançado", "Médio", "Defesa", "Guarda-redes")
    val POSICOES_CURTAS = listOf("AV", "MC", "DF", "GR")
    val PES = listOf("Direito", "Esquerdo", "Ambos")
    val SITUACOES = listOf("Ativo", "Lesionado", "Indisponível")

    fun posicao(valor: Int?): String = POSICOES.getOrNull(valor ?: -1) ?: "—"
    fun posicaoCurta(valor: Int?): String = POSICOES_CURTAS.getOrNull(valor ?: -1) ?: "?"
    fun pe(valor: Int?): String = PES.getOrNull(valor ?: -1) ?: "—"
    fun situacao(valor: Int?): String = SITUACOES.getOrNull(valor ?: -1) ?: "—"

    fun estadoEpoca(valor: Int?): String = when (valor) {
        0 -> "Inscrições abertas"
        1 -> "A decorrer"
        2 -> "Terminada"
        else -> "Sem época"
    }

    fun estadoProposta(valor: Int): String = when (valor) {
        0 -> "À espera do clube"
        1 -> "À espera do jogador"
        2 -> "Aceite"
        3 -> "Recusada"
        4 -> "Cancelada"
        else -> "—"
    }

    fun estadoJogo(valor: Int): String = when (valor) {
        0 -> "Marcado"
        1 -> "A decorrer"
        2 -> "Terminado"
        3 -> "Adiado"
        4 -> "Cancelado"
        else -> "—"
    }

    fun tipoTransferencia(kind: String): String = when (kind) {
        "TRANSFERENCIA" -> "Transferência"
        "ADESAO" -> "Entrada como jogador livre"
        "SAIDA" -> "Saída (ficou livre)"
        else -> kind
    }

    fun zona(zone: String?): String? = when (zone) {
        "PROMOTION" -> "Subida"
        "RELEGATION" -> "Descida"
        else -> null
    }

    /** Diferença de golos com sinal: "+3", "0", "−2". */
    fun comSinal(n: Int): String = when {
        n > 0 -> "+$n"
        n < 0 -> "−${-n}"
        else -> "0"
    }
}

/** Jogador disponível para o onze ou para os eventos do jogo. */
data class JogadorPlantel(val id: String, val nome: String, val posicao: Int)

object RegrasOnze {
    /**
     * Candidatos a uma posição do onze: primeiro os da posição, depois (se [todos]) os restantes,
     * sem os já escolhidos. Ordena por nome.
     */
    fun candidatos(
        plantel: List<JogadorPlantel>,
        role: Int,
        escolhidos: Set<String>,
        todos: Boolean
    ): List<JogadorPlantel> {
        val livres = plantel.filter { it.id !in escolhidos }
        val daPosicao = livres.filter { it.posicao == role }.sortedBy { it.nome.lowercase() }
        if (!todos) return daPosicao
        return daPosicao + livres.filter { it.posicao != role }.sortedBy { it.nome.lowercase() }
    }
}

/** Validação dos eventos que um administrador regista no fim do jogo. */
object RegrasEventos {
    const val MINUTO_MAXIMO = 130

    /**
     * Devolve a primeira mensagem de erro ou nulo se estiver tudo bem. Segue as mesmas regras
     * da API (MatchDetailsService.ValidateEvents); [titulares] vazio quando não há onze.
     */
    fun validar(golosEquipa: Int, eventos: MatchEventsDto, titulares: Set<String> = emptySet()): String? {
        if (eventos.fouls < 0 || eventos.fouls > 200) return "O número de faltas deve estar entre 0 e 200."
        if (eventos.goals.size > golosEquipa) {
            return "Registaste ${eventos.goals.size} golos, mas a equipa marcou $golosEquipa."
        }
        val minutos = eventos.goals.map { it.minute } + eventos.cards.map { it.minute } +
            eventos.substitutions.map { it.minute }
        if (minutos.any { it < 0 || it > MINUTO_MAXIMO }) {
            return "Os minutos têm de estar entre 0 e $MINUTO_MAXIMO."
        }
        if (eventos.goals.any { it.scorerId != null && it.scorerId == it.assistId }) {
            return "Um jogador não pode assistir o próprio golo."
        }
        val cartoes = eventos.cards.groupBy { it.playerId }
        if (cartoes.values.any { l -> l.count { it.type == 1 } > 1 || l.count { it.type == 0 } > 2 }) {
            return "Um jogador tem no máximo dois amarelos e um vermelho."
        }
        val entraram = mutableSetOf<String>()
        for (s in eventos.substitutions.sortedBy { it.minute }) {
            if (s.playerInId == s.playerOutId) {
                return "Numa substituição, quem sai e quem entra têm de ser jogadores diferentes."
            }
            if (s.playerInId in titulares || !entraram.add(s.playerInId)) {
                return "Quem entra numa substituição tem de vir do banco (e só entra uma vez)."
            }
        }
        return null
    }
}

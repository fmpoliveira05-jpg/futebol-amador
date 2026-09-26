package com.example.amfootball.competicao

import java.time.DayOfWeek
import java.time.LocalDate
import java.time.LocalDateTime
import java.time.OffsetDateTime
import java.time.YearMonth
import java.time.ZoneId
import java.time.format.DateTimeFormatter
import java.util.Locale

/** Conversão das datas da API (com ou sem fuso) para hora local. */
object Datas {
    private val PT = Locale.forLanguageTag("pt-PT")
    private val DATA = DateTimeFormatter.ofPattern("dd/MM/yyyy", PT)
    private val DATA_HORA = DateTimeFormatter.ofPattern("dd/MM/yyyy HH:mm", PT)

    fun parse(texto: String?): LocalDateTime? {
        if (texto.isNullOrBlank()) return null
        return try {
            LocalDateTime.parse(texto)
        } catch (e: Exception) {
            try {
                OffsetDateTime.parse(texto).atZoneSameInstant(ZoneId.systemDefault()).toLocalDateTime()
            } catch (e: Exception) {
                try {
                    LocalDate.parse(texto.take(10)).atStartOfDay()
                } catch (e: Exception) {
                    null
                }
            }
        }
    }

    fun data(texto: String?): String = parse(texto)?.format(DATA) ?: "—"

    fun dataHora(texto: String?): String = parse(texto)?.format(DATA_HORA) ?: "—"

    fun dataHora(valor: LocalDateTime): String = valor.format(DATA_HORA)
}

/** Feriados nacionais obrigatórios de Portugal (os 13 do Código do Trabalho, sem Carnaval nem municipais). */
object Feriados {

    /** Domingo de Páscoa (algoritmo de Meeus/Jones/Butcher). */
    fun pascoa(ano: Int): LocalDate {
        val a = ano % 19
        val b = ano / 100
        val c = ano % 100
        val d = b / 4
        val e = b % 4
        val f = (b + 8) / 25
        val g = (b - f + 1) / 3
        val h = (19 * a + b - d - g + 15) % 30
        val i = c / 4
        val k = c % 4
        val l = (32 + 2 * e + 2 * i - h - k) % 7
        val m = (a + 11 * h + 22 * l) / 451
        val mes = (h + l - 7 * m + 114) / 31
        val dia = ((h + l - 7 * m + 114) % 31) + 1
        return LocalDate.of(ano, mes, dia)
    }

    /** Os 13 feriados de um ano, por ordem de data. */
    fun doAno(ano: Int): Map<LocalDate, String> {
        val p = pascoa(ano)
        return listOf(
            LocalDate.of(ano, 1, 1) to "Ano Novo",
            p.minusDays(2) to "Sexta-feira Santa",
            p to "Páscoa",
            LocalDate.of(ano, 4, 25) to "Dia da Liberdade",
            LocalDate.of(ano, 5, 1) to "Dia do Trabalhador",
            p.plusDays(60) to "Corpo de Deus",
            LocalDate.of(ano, 6, 10) to "Dia de Portugal",
            LocalDate.of(ano, 8, 15) to "Assunção de Nossa Senhora",
            LocalDate.of(ano, 10, 5) to "Implantação da República",
            LocalDate.of(ano, 11, 1) to "Dia de Todos os Santos",
            LocalDate.of(ano, 12, 1) to "Restauração da Independência",
            LocalDate.of(ano, 12, 8) to "Imaculada Conceição",
            LocalDate.of(ano, 12, 25) to "Natal"
        ).sortedBy { it.first }.toMap()
    }
}

/** Grelha mensal de segunda a domingo. */
object GrelhaMes {
    val DIAS_SEMANA = listOf("Seg", "Ter", "Qua", "Qui", "Sex", "Sáb", "Dom")

    private val MESES = listOf(
        "janeiro", "fevereiro", "março", "abril", "maio", "junho",
        "julho", "agosto", "setembro", "outubro", "novembro", "dezembro"
    )

    /** Semanas completas (4 a 6) que cobrem o mês; os dias de outros meses servem de enchimento. */
    fun semanas(mes: YearMonth): List<List<LocalDate>> {
        val inicio = mes.atDay(1).let { it.minusDays((it.dayOfWeek.value - DayOfWeek.MONDAY.value).toLong()) }
        val fim = mes.atEndOfMonth().let { it.plusDays((DayOfWeek.SUNDAY.value - it.dayOfWeek.value).toLong()) }
        val dias = generateSequence(inicio) { it.plusDays(1) }.takeWhile { !it.isAfter(fim) }.toList()
        return dias.chunked(7)
    }

    /** "Setembro de 2026". */
    fun titulo(mes: YearMonth): String {
        val nome = MESES[mes.monthValue - 1]
        return "${nome.replaceFirstChar { it.uppercase() }} de ${mes.year}"
    }
}

/** Tipos de bolinha de um dia, pela ordem da legenda. */
enum class TipoMarca(val texto: String) {
    FERIADO("Feriado"),
    AMIGAVEL("Amigável marcado"),
    LIGA("Jogo da liga marcado"),
    TERMINADO("Terminado"),
    CANCELADO("Cancelado"),
    ADIADO("Adiado")
}

object MarcasCalendario {
    /**
     * Cor de um jogo: pelo estado (0 marcado, 1 a decorrer, 2 terminado, 3 adiado, 4 cancelado)
     * e, se ainda está por jogar, pelo tipo (liga ou amigável).
     */
    fun tipoDoJogo(estado: Int, competitivo: Boolean): TipoMarca = when (estado) {
        2 -> TipoMarca.TERMINADO
        3 -> TipoMarca.ADIADO
        4 -> TipoMarca.CANCELADO
        else -> if (competitivo) TipoMarca.LIGA else TipoMarca.AMIGAVEL
    }

    fun tipoDoHistorico(kind: String): TipoMarca =
        if (kind == "CANCELLED") TipoMarca.CANCELADO else TipoMarca.ADIADO

    /** Marcas de um dia, sem repetições e pela ordem da legenda. */
    fun doDia(feriado: Boolean, jogos: List<TipoMarca>, historico: List<TipoMarca>): List<TipoMarca> {
        val tipos = buildSet {
            if (feriado) add(TipoMarca.FERIADO)
            addAll(jogos)
            addAll(historico)
        }
        return TipoMarca.entries.filter { it in tipos }
    }
}

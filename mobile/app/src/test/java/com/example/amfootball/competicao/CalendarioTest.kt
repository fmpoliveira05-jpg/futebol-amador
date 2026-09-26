package com.example.amfootball.competicao

import org.junit.Assert.assertEquals
import org.junit.Assert.assertNull
import org.junit.Assert.assertTrue
import org.junit.Test
import java.time.DayOfWeek
import java.time.LocalDate
import java.time.LocalDateTime
import java.time.YearMonth

class FeriadosTest {

    @Test
    fun pascoa_datasConhecidas() {
        assertEquals(LocalDate.of(2024, 3, 31), Feriados.pascoa(2024))
        assertEquals(LocalDate.of(2025, 4, 20), Feriados.pascoa(2025))
        assertEquals(LocalDate.of(2026, 4, 5), Feriados.pascoa(2026))
        assertEquals(LocalDate.of(2027, 3, 28), Feriados.pascoa(2027))
    }

    @Test
    fun doAno_tem13FeriadosComOsMoveis() {
        val f = Feriados.doAno(2026)
        assertEquals(13, f.size)
        assertEquals("Sexta-feira Santa", f[LocalDate.of(2026, 4, 3)])
        assertEquals("Páscoa", f[LocalDate.of(2026, 4, 5)])
        assertEquals("Corpo de Deus", f[LocalDate.of(2026, 6, 4)])
        assertEquals("Implantação da República", f[LocalDate.of(2026, 10, 5)])
        assertEquals("Natal", f[LocalDate.of(2026, 12, 25)])
    }

    @Test
    fun doAno_estaOrdenado() {
        val datas = Feriados.doAno(2025).keys.toList()
        assertEquals(datas.sorted(), datas)
    }
}

class GrelhaMesTest {

    @Test
    fun semanas_comecamASegundaEAcabamAoDomingo() {
        val semanas = GrelhaMes.semanas(YearMonth.of(2026, 9))
        assertTrue(semanas.all { it.size == 7 })
        assertEquals(DayOfWeek.MONDAY, semanas.first().first().dayOfWeek)
        assertEquals(DayOfWeek.SUNDAY, semanas.last().last().dayOfWeek)
        // setembro de 2026 começa a uma terça: a grelha começa na segunda 31 de agosto
        assertEquals(LocalDate.of(2026, 8, 31), semanas.first().first())
        assertEquals(LocalDate.of(2026, 10, 4), semanas.last().last())
    }

    @Test
    fun semanas_cobremTodosOsDiasDoMes() {
        for (m in 1..12) {
            val mes = YearMonth.of(2027, m)
            val dias = GrelhaMes.semanas(mes).flatten().filter { YearMonth.from(it) == mes }
            assertEquals(mes.lengthOfMonth(), dias.size)
            assertTrue(GrelhaMes.semanas(mes).size in 4..6)
        }
    }

    @Test
    fun titulo_emPortugues() {
        assertEquals("Setembro de 2026", GrelhaMes.titulo(YearMonth.of(2026, 9)))
        assertEquals("Março de 2027", GrelhaMes.titulo(YearMonth.of(2027, 3)))
    }
}

class MarcasCalendarioTest {

    @Test
    fun tipoDoJogo_peloEstadoEPeloTipo() {
        assertEquals(TipoMarca.AMIGAVEL, MarcasCalendario.tipoDoJogo(0, competitivo = false))
        assertEquals(TipoMarca.LIGA, MarcasCalendario.tipoDoJogo(0, competitivo = true))
        assertEquals(TipoMarca.TERMINADO, MarcasCalendario.tipoDoJogo(2, competitivo = true))
        assertEquals(TipoMarca.ADIADO, MarcasCalendario.tipoDoJogo(3, competitivo = false))
        assertEquals(TipoMarca.CANCELADO, MarcasCalendario.tipoDoJogo(4, competitivo = true))
    }

    @Test
    fun doDia_semRepeticoesENaOrdemDaLegenda() {
        val marcas = MarcasCalendario.doDia(
            feriado = true,
            jogos = listOf(TipoMarca.TERMINADO, TipoMarca.AMIGAVEL, TipoMarca.TERMINADO),
            historico = listOf(MarcasCalendario.tipoDoHistorico("CANCELLED"))
        )
        assertEquals(listOf(TipoMarca.FERIADO, TipoMarca.AMIGAVEL, TipoMarca.TERMINADO, TipoMarca.CANCELADO), marcas)
    }

    @Test
    fun datas_aceitamComESemFuso() {
        assertEquals(LocalDateTime.of(2026, 10, 3, 15, 0), Datas.parse("2026-10-03T15:00:00"))
        assertEquals(LocalDate.of(2000, 5, 1), Datas.parse("2000-05-01")?.toLocalDate())
        assertTrue(Datas.parse("2026-10-03T15:00:00Z") != null)
        assertNull(Datas.parse(""))
        assertNull(Datas.parse("isto não é uma data"))
    }
}

class RegrasTest {

    private val plantel = listOf(
        JogadorPlantel("gr", "Rui", 3),
        JogadorPlantel("d1", "Bruno", 2),
        JogadorPlantel("d2", "André", 2),
        JogadorPlantel("m1", "Carlos", 1)
    )

    @Test
    fun candidatos_primeiroDaPosicaoDepoisOsOutros() {
        assertEquals(listOf("d2", "d1"), RegrasOnze.candidatos(plantel, 2, emptySet(), todos = false).map { it.id })
        assertEquals(
            listOf("d2", "m1", "gr"),
            RegrasOnze.candidatos(plantel, 2, setOf("d1"), todos = true).map { it.id }
        )
    }

    @Test
    fun eventos_validos() {
        val e = MatchEventsDto(
            fouls = 10,
            goals = listOf(GoalEventDto("m1", "d1", 20), GoalEventDto(null, null, 70)),
            cards = listOf(CardEventDto("d1", 0, 30), CardEventDto("d1", 0, 60)),
            substitutions = listOf(SubstitutionEventDto("m1", "d2", 65))
        )
        assertNull(RegrasEventos.validar(2, e, titulares = setOf("m1", "d1")))
    }

    @Test
    fun eventos_invalidos() {
        assertTrue(RegrasEventos.validar(1, MatchEventsDto(goals = listOf(GoalEventDto("a", null, 1), GoalEventDto("b", null, 2)))) != null)
        assertTrue(RegrasEventos.validar(1, MatchEventsDto(goals = listOf(GoalEventDto("a", "a", 1)))) != null)
        assertTrue(RegrasEventos.validar(0, MatchEventsDto(fouls = 201)) != null)
        assertTrue(RegrasEventos.validar(0, MatchEventsDto(cards = List(2) { CardEventDto("a", 1, 10) })) != null)
        assertTrue(RegrasEventos.validar(0, MatchEventsDto(cards = listOf(CardEventDto("a", 0, 131)))) != null)
        // quem entra tem de vir do banco e só entra uma vez
        assertTrue(RegrasEventos.validar(0, MatchEventsDto(substitutions = listOf(SubstitutionEventDto("a", "b", 50))), setOf("a", "b")) != null)
        assertTrue(
            RegrasEventos.validar(
                0,
                MatchEventsDto(substitutions = listOf(SubstitutionEventDto("a", "c", 50), SubstitutionEventDto("b", "c", 60)))
            ) != null
        )
    }

    @Test
    fun textos() {
        assertEquals("+3", Textos.comSinal(3))
        assertEquals("0", Textos.comSinal(0))
        assertEquals("−2", Textos.comSinal(-2))
        assertEquals("Subida", Textos.zona("PROMOTION"))
        assertNull(Textos.zona(null))
        assertEquals("Guarda-redes", Textos.posicao(3))
        assertEquals("—", Textos.posicao(9))
    }

    @Test
    fun filtrosMercado_soEnviaOsPreenchidos() {
        assertEquals(emptyMap<String, String>(), FiltrosMercado().paraQuery())
        val q = FiltrosMercado(temEquipa = false, nacionalidade = " Portugal ", posicao = 1, soListados = true).paraQuery()
        assertEquals(mapOf("hasTeam" to "false", "nationality" to "Portugal", "position" to "1", "onlyListed" to "true"), q)
    }
}

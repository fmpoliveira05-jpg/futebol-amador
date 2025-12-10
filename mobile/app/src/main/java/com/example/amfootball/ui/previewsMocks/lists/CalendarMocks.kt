package com.example.amfootball.ui.previewsMocks.lists

import com.example.amfootball.data.remote.dtos.match.InfoMatchCalendar
import com.example.amfootball.data.remote.dtos.support.PitchInfo
import com.example.amfootball.data.remote.dtos.support.TeamStatisticsDto
import com.example.amfootball.ui.previewsMocks.lists.CalendarMocks.filterActions
import com.example.amfootball.ui.previewsMocks.lists.CalendarMocks.itemActions
import com.example.amfootball.ui.previewsMocks.lists.CalendarMocks.listNormal
import java.time.LocalDateTime

/**
 * Fornece dados fictícios (Mocks) e implementações vazias (Stubs) de ações para o ecrã de Calendário de Jogos.
 *
 * Este objeto é utilizado para popular as pré-visualizações do Jetpack Compose (Previews) e para testes de UI,
 * isolando a interface da lógica de negócio real.
 *
 * @property filterActions Uma implementação "No-Op" (No Operation) das ações de filtro.
 * Todos os callbacks são definidos como funções vazias `{}`, permitindo renderizar a UI de filtros
 * sem causar erros de *NullPointer* ou disparar lógica indesejada ao interagir com os campos.
 *
 * @property itemActions Uma implementação "No-Op" das ações dos itens da lista.
 * Permite renderizar os cartões de jogo e simular cliques (Cancelar, Adiar, Iniciar) sem efeitos colaterais.
 *
 * @property listNormal Retorna uma lista dinâmica de [InfoMatchCalendar] para teste.
 * Utiliza um *getter* personalizado para gerar as datas (`LocalDateTime.now()`) no momento do acesso,
 * garantindo que os dados de teste (jogos passados vs futuros) permanecem cronologicamente corretos
 * independentemente do dia em que os testes são executados.
 *
 * **Cenários incluídos na lista:**
 * 1. Jogo Passado ("DONE"): Realizado há 3 dias, jogado "Fora", derrota (1-3).
 * 2. Jogo Futuro ("SCHEDULED"): Agendado para daqui a 5 dias, jogo em "Casa".
 */
object CalendarMocks {
    val filterActions =
        _root_ide_package_.com.example.amfootball.ui.actions.filters.FilterCalendarActions(
            onNameChange = {},
            onMinDateGameChange = {},
            onMaxDateGameChange = {},
            onGameLocalChange = {},
            onTypeMatchChange = {},
            onFinishMatch = {},
            onButtonFilterActions = _root_ide_package_.com.example.amfootball.ui.actions.filters.ButtonFilterActions(
                onFilterApply = {},
                onFilterClean = {}
            )
        )

    val itemActions =
        _root_ide_package_.com.example.amfootball.ui.actions.itemsList.ItemsCalendarActions(
            onCancelMatch = { _, _ -> },
            onPostPoneMatch = { _, _ -> },
            onStartMatch = {},
            onFinishMatch = { _, _ -> }
        )

    val listNormal: List<InfoMatchCalendar>
        get() = listOf(
            InfoMatchCalendar(
                idMatch = "1",
                matchStatusId = 2, // DONE
                rawGameDate = LocalDateTime.now().minusDays(3).toString(),
                typeMatchBool = true,
                matchResultId = 0,
                pitchGame = PitchInfo("Estádio Municipal", "Rua Principal"),
                team = TeamStatisticsDto("t1", "Vitória SC", 3, ""),
                opponent = TeamStatisticsDto("t2", "Dragões FC", 1, ""),
                isHome = false
            ),
            InfoMatchCalendar(
                idMatch = "2",
                matchStatusId = 0, // SCHEDULED
                rawGameDate = LocalDateTime.now().plusDays(5).toString(),
                typeMatchBool = false,
                matchResultId = -1,
                pitchGame = PitchInfo("Campo de Treinos", "Avenida Secundária"),
                team = TeamStatisticsDto("t1", "Vitória SC", 0, ""),
                opponent = TeamStatisticsDto("t3", "Sporting CP", 0, ""),
                isHome = true
            )
        )
}
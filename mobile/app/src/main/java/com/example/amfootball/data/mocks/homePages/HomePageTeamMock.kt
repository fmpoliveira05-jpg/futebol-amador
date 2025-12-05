package com.example.amfootball.data.mocks.homePages

import com.example.amfootball.data.dtos.support.TeamDto

/**
 * Objeto utilitário que fornece dados fictícios (Mock Data) de Equipas para o ecrã Principal (Home Page).
 *
 * Este objeto isola a criação de instâncias de [TeamDto] para fins de testes e pré-visualização de UI,
 * garantindo consistência nos dados apresentados nos componentes visuais (ex: Team Card ou Header da Home).
 */
object HomePageTeamMock {

    /**
     * Uma instância estática de [TeamDto] representando uma equipa completa e válida ("Happy Path").
     *
     * **Dados do Mock:**
     * - **Nome:** "Porto Renegades"
     * - **ID:** "1"
     * - **Imagem:** String vazia (deve acionar o placeholder de imagem na UI).
     *
     * Útil para validar layouts que exibem o resumo da equipa associada ao utilizador.
     */
    val mockTeam = TeamDto(
        id = "1",
        name = "Porto Renegades",
        image = ""
    )
}
package com.example.amfootball.ui.screens.privacidade

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import com.example.amfootball.R

/**
 * Versão da Política de Privacidade aceite no registo. Tem de ser igual a `Rgpd:VersaoPolitica`
 * na API e à da web (`VERSAO_POLITICA_PRIVACIDADE`); se mudar, mudam as três.
 */
const val VERSAO_POLITICA_PRIVACIDADE = "2026-09-27"

/**
 * Política de Privacidade (RGPD, art. 13.º), em resumo. Pública: abre a partir do registo, antes
 * de a conta existir, e das definições. A versão completa está na página /privacidade do site.
 */
@Composable
fun PoliticaPrivacidadeScreen(modifier: Modifier = Modifier) {
    val seccoes = listOf(
        R.string.rgpd_p_responsavel_titulo to R.string.rgpd_p_responsavel,
        R.string.rgpd_p_dados_titulo to R.string.rgpd_p_dados,
        R.string.rgpd_p_fins_titulo to R.string.rgpd_p_fins,
        R.string.rgpd_p_partilha_titulo to R.string.rgpd_p_partilha,
        R.string.rgpd_p_prazos_titulo to R.string.rgpd_p_prazos,
        R.string.rgpd_p_direitos_titulo to R.string.rgpd_p_direitos,
        R.string.rgpd_p_seguranca_titulo to R.string.rgpd_p_seguranca,
    )

    Column(
        modifier = modifier
            .fillMaxSize()
            .verticalScroll(rememberScrollState())
            .padding(16.dp),
        verticalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        Text(stringResource(R.string.rgpd_titulo_politica), style = MaterialTheme.typography.headlineSmall)
        Text(
            stringResource(R.string.rgpd_versao, VERSAO_POLITICA_PRIVACIDADE),
            style = MaterialTheme.typography.bodySmall
        )
        for ((titulo, texto) in seccoes) {
            Text(stringResource(titulo), style = MaterialTheme.typography.titleMedium)
            Text(stringResource(texto), style = MaterialTheme.typography.bodyMedium)
        }
    }
}

package com.example.amfootball.ui.components.lists

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.LazyListScope
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Inbox
import androidx.compose.material3.ElevatedCard
import androidx.compose.material3.FilledTonalButton
import androidx.compose.material3.Icon
import androidx.compose.material3.ListItem
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import com.example.amfootball.R
import com.example.amfootball.ui.components.buttons.AcceptButton
import com.example.amfootball.ui.components.buttons.RejectButton
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton

/**
 * Container principal que exibe uma lista dinâmica de itens ([LazyColumn]) com suporte a cabeçalho de filtro,
 * estados vazios e paginação.
 *
 * Este componente orquestra a UI delegando a renderização para funções de extensão do [LazyListScope]:
 * - [filterSection]: Renderiza a área de filtros no topo.
 * - [emptyListState]: Renderiza uma mensagem e ícone se a lista estiver vazia.
 * - [populatedListContent]: Renderiza os itens da lista e o botão de "Carregar Mais".
 *
 * @param T O tipo de dados dos itens na lista.
 * @param list A lista de dados a ser exibida.
 * @param filterSection Slot Composable para a secção de filtros (opcional).
 * @param listItems Slot Composable para renderizar a UI de um item individual da lista.
 * @param isValidShowMore Booleano que indica se o botão "Ver Mais" deve estar visível (geralmente true se houver mais páginas).
 * @param showMoreItems Callback executado ao clicar no botão "Ver Mais".
 * @param itemSpacing O espaçamento vertical entre os itens da lista em dp (padrão: 12).
 * @param messageEmptyList A mensagem a ser exibida quando a lista estiver vazia.
 */
@Composable
fun<T> ListSurface(
    list: List<T>,
    filterSection: @Composable () -> Unit = {},
    listItems: @Composable (T) -> Unit,
    isValidShowMore: Boolean = false,
    showMoreItems: () -> Unit = {},
    itemSpacing: Int = 12,
    messageEmptyList: String
) {
    Surface {
        LazyColumn(
            contentPadding = PaddingValues(horizontal = 16.dp, vertical = 8.dp)
        ) {
            filterSection(filterSection = filterSection)

            if (list.isEmpty()) {
               emptyListState(message = messageEmptyList)
            } else {
                populatedListContent(
                    list = list,
                    itemRender = listItems,
                    itemSpacing = itemSpacing,
                    isValidShowMore = isValidShowMore,
                    onShowMoreClick = showMoreItems
                )
            }
        }
    }
}

/**
 * Extensão do [LazyListScope] para renderizar a secção de filtros.
 * Adiciona um espaçador vertical após o conteúdo do filtro.
 *
 * @param filterSection O conteúdo Composable do filtro.
 */
private fun LazyListScope.filterSection(
    filterSection: @Composable () -> Unit
) {
    if(filterSection != {}) {
        item {
            filterSection()
            Spacer(Modifier.height(16.dp))
        }
    }
}

/**
 * Extensão do [LazyListScope] para renderizar o estado de lista vazia.
 * Exibe um ícone [Icons.Default.Inbox] e uma mensagem centralizada.
 *
 * @param message A mensagem a ser exibida ao utilizador.
 */
private fun LazyListScope.emptyListState(
    message: String
) {
    item {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .padding(vertical = 64.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            Icon(
                imageVector = Icons.Default.Inbox,
                contentDescription = null,
                modifier = Modifier.size(72.dp),
                tint = MaterialTheme.colorScheme.onSurfaceVariant
            )

            Text(
                text = message,
                style = MaterialTheme.typography.headlineSmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                textAlign = TextAlign.Center
            )
        }
    }
}

/**
 * Extensão do [LazyListScope] para renderizar o conteúdo da lista quando esta tem dados.
 *
 * Responsável por:
 * 1. Iterar sobre a [list] e renderizar cada item usando [itemRender].
 * 2. Adicionar espaçamento ([itemSpacing]) entre os itens.
 * 3. Adicionar o botão "Ver Mais" no final da lista se [isValidShowMore] for verdadeiro.
 *
 * @param list A lista de itens a renderizar.
 * @param itemRender O Composable que define a aparência de cada item.
 * @param itemSpacing Espaçamento em dp entre itens.
 * @param isValidShowMore Controla a visibilidade do botão de paginação.
 * @param onShowMoreClick Ação ao clicar no botão de paginação.
 */
private fun <T> LazyListScope.populatedListContent(
    list: List<T>,
    itemRender: @Composable (T) -> Unit,
    itemSpacing: Int,
    isValidShowMore: Boolean = false,
    onShowMoreClick: () -> Unit
) {
    items(list) { item ->
        itemRender(item)
        Spacer(Modifier.height(itemSpacing.dp))
    }

    if (isValidShowMore) {
        item {
            FilledTonalButton(
                onClick = onShowMoreClick,
                modifier = Modifier.fillMaxWidth()
            ) {
                Text(
                    text = stringResource(id = R.string.button_view_more),
                    style = MaterialTheme.typography.bodyMedium,
                    maxLines = 1
                )
            }
        }
    }
}

/**
 * Container genérico para um item individual da lista, estilizado como um [ElevatedCard].
 *
 * Utiliza o [ListItem] do Material 3 e oferece slots customizados para título,
 * conteúdo à esquerda (leading), conteúdo de suporte e rodapé opcional.
 *
 * @param T O tipo de dados do item.
 * @param item O objeto de dados a ser exibido.
 * @param title Função que extrai a string principal do título a partir do item.
 * @param overline Slot Composable para exibir conteúdo sobre o título.
 * @param leading Slot Composable que exibe o conteúdo inicial (ex: imagem de perfil).
 * @param supporting Slot Composable que exibe informações adicionais abaixo do título.
 * @param trailing Slot Composable que exibe conteúdo no final da linha (ex: botões de ação).
 * @param underneathItems Slot Composable opcional para exibir conteúdo abaixo do [ListItem] (ex: uma linha de botões).
 */
@Composable
fun<T> GenericListItem(
    item: T,
    title: (T) -> String,
    overline: @Composable () -> Unit = {},
    leading: @Composable (T) -> Unit = {},
    supporting: @Composable (T) -> Unit = {},
    trailing: @Composable () -> Unit = {},
    underneathItems: (@Composable () -> Unit)? = null
) {
    ElevatedCard(modifier = Modifier.fillMaxWidth()) {
        Column {
            ListItem(
                headlineContent = {
                    Text(
                        text = title(item),
                        style = MaterialTheme.typography.titleLarge,
                        maxLines = 2,
                        overflow = TextOverflow.Ellipsis
                    )
                },
                overlineContent = {
                    overline()
                },
                supportingContent = {
                    supporting(item)
                },
                leadingContent = {
                    leading(item)
                },
                trailingContent = {
                    trailing()
                }
            )
        }

        if (underneathItems != null) {
            Row(
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.Center,
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 8.dp, vertical = 4.dp)
            ) {
                underneathItems()
            }
        }
    }
}

/**
 * Um agrupador de botões para ações comuns de listas, como pedidos de adesão ou detalhes.
 *
 * Exibe os botões [AcceptButton], [RejectButton] e [ShowMoreInfoButton] lado a lado.
 *
 * @param accept Callback executado ao pressionar o botão de aceitar.
 * @param reject Callback executado ao pressionar o botão de rejeitar.
 * @param showMore Callback executado ao pressionar o botão de ver mais detalhes.
 */
@Composable
fun ItemAcceptRejectAndShowMore(
    accept: () -> Unit,
    reject: () -> Unit,
    showMore: () -> Unit,
) {
    Row(
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(4.dp),
        modifier = Modifier.padding(start = 8.dp)
    ) {
        AcceptButton(accept = accept)

        RejectButton(reject = reject)

        ShowMoreInfoButton(
            showMoreDetails = showMore,
            contentDescription = stringResource(id = R.string.list_teams_view_team)
        )
    }
}
package com.example.amfootball.ui.screens.MembershipRequest

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.ui.components.BackTopBar
import com.example.amfootball.ui.components.InputFields.TextFieldOutline

//Isto não é um screen é a ação de um botão
//Depois arranjar forma de adaptar isto para dar tanto para a equipa como os players utilizarem
@Composable
fun SendMemberShipRequest(navHostController: NavHostController) {
    Scaffold(
        topBar = {
            BackTopBar(
                navHostController = navHostController,
                title = stringResource(id = R.string.title_page_create_team)
            )},
        content = { paddingValues ->
            ContentSendMemberShipRequest(
                modifier = Modifier
                    .padding(paddingValues)
                    .padding(16.dp),
                navHostController = navHostController
            )
        }
    )
}

@Composable
private fun ContentSendMemberShipRequest(modifier: Modifier = Modifier,
                                         navHostController: NavHostController) {
    Column(
        modifier = modifier,
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        FieldsSendMemberShipRequest()
    }
}

@Composable
private fun FieldsSendMemberShipRequest() {
    /*
    * label: String,
                           value: String,
                           isSingleLine: Boolean = true,
                           onValueChange: (String) -> Unit = {},
                           isReadOnly: Boolean = false,
                           isRequired: Boolean = false,
                           isError: Boolean = false,
                           errorMessage: String = stringResource(id = R.string.mandatory_field)
)
    * */
    val receiver by remember { mutableStateOf("Recetor") }


    TextFieldOutline(
        label = "Recetor",
        value = receiver,
        isReadOnly = true
    )


}

@Preview(
    name = "Enviar pedido de adesão - PT",
    locale = "pt-rPT",
    showBackground = true
)
@Preview(
    name = "Send MemberShip Request - EN",
    locale = "en",
    showBackground = true
)
@Composable
fun SendMemberShipRequestPreview() {
    SendMemberShipRequest(navHostController = rememberNavController())
}
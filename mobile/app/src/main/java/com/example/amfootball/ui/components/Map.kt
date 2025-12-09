package com.example.amfootball.ui.components

//Depois reutilizar para os mapas

//Nota não está a ser utilizado
/*
@SuppressLint("MissingPermission")
@Composable
fun MapScreenContainer(onBack: () -> Unit, viewModel: MainViewModel = viewModel()) {
    val coroutineScope = rememberCoroutineScope()

    val ctx = LocalContext.current

    val fusedLocationClient = remember { LocationServices.getFusedLocationProviderClient(ctx) }

    var userLat by remember { mutableStateOf(41.1579) } // fallback coords
    var userLng by remember { mutableStateOf(-8.6291) }

    val requestPermissionLauncher =
        rememberLauncherForActivityResult(ActivityResultContracts.RequestPermission()) { accepted ->
        }

    LaunchedEffect(Unit) {
        requestPermissionLauncher.launch(Manifest.permission.ACCESS_FINE_LOCATION)

        try {
            fusedLocationClient.lastLocation.addOnSuccessListener { loc: Location? ->
                loc?.let {
                    userLat = it.latitude
                    userLng = it.longitude
                }
            }.addOnFailureListener {
            }
        } catch (e: Exception) {
            // ignora caso calhe
        }
    }

    val matches by viewModel.matches.collectAsState()
    var nearbyMatches by remember { mutableStateOf<List<Match>>(emptyList()) }

    Column(modifier = Modifier.fillMaxSize()) {
        Box(modifier = Modifier.weight(1f)) {
            val cameraPositionState = rememberCameraPositionState {
                position = CameraPosition.fromLatLngZoom(LatLng(userLat, userLng), 12f)
            }
            GoogleMap(
                modifier = Modifier.fillMaxSize(),
                cameraPositionState = cameraPositionState,
                properties = MapProperties(isMyLocationEnabled = true)
            ) {
                // user marker
                Marker(
                    state = MarkerState(LatLng(userLat, userLng)),
                    title = "Você",
                    snippet = "Localização atual"
                )

                // match markers
                matches.forEach { m ->
                    Marker(
                        state = MarkerState(LatLng(m.location.lat, m.location.lng)),
                        title = "Partida: ${m.teamAId.take(6)} vs ${m.teamBId.take(6)}",
                        snippet = m.location.address ?: "Campo"
                    )
                }
            }
        }

        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(8.dp),
            horizontalArrangement = Arrangement.SpaceBetween
        ) {
            Button(onClick = {
                nearbyMatches = viewModel.findNearbyMatches(userLat, userLng, radiusKm = 15.0)
            }) { Text("Procurar partidas próximas") }
            Button(onClick = {
                coroutineScope.launch { viewModel.scheduleRandomNearbyMatch(userLat, userLng) }
            }) { Text("Gerar partida próxima (demo)") }
            Button(onClick = onBack) { Text("Voltar") }
        }
        Spacer(modifier = Modifier.height(8.dp))
    }
}
 */
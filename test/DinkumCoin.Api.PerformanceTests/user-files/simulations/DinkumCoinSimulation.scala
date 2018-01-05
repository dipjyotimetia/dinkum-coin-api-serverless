package crownbet.qa.gatling.example

import io.gatling.core.scenario.Simulation
import io.gatling.core.Predef._
import io.gatling.http.Predef._

import scala.concurrent.duration._


class DinkumCoinSimulation extends Simulation {

  val HOST = "https://sywq3pqw4c.execute-api.ap-southeast-2.amazonaws.com"
  var RESOURCE = "/dev/api/wallets"
  val QueryParams = Map[String, String]()


  private val httpConfig = http.baseURL(HOST)

  private val scn = scenario("DinkumCoinSimulation").
    exec(http("open").
      get(RESOURCE)).
    pause(1)

  setUp(scn.inject(constantUsersPerSec(2) during (1 minutes))).protocols(httpConfig)
    .assertions(global.successfulRequests.percent.is(100))


}

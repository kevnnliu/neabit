# Near Orbit
### Kevin Liu, Dhruba Ghosh
This repository contains the Near Orbit prototype, our final project submission for VR DeCal, Spring 2019, UC Berkeley. This initial version used Photon Unity Networking under a peer-to-peer multiplayer networking model and the Oculus SDK to process VR inputs. We have since rebuilt the project from scratch, more details below. 

## Official Project Details
Near Orbit aims to be a VR multiplayer game where players pilot spacejets with unique weaponry and abilities. The networking is under a server-authoritative server-client model where a server instance simulates and validates in-game events and actions. This promotes consistency across clients and mitigates server authority abuse as would be possible in a peer-to-peer model.

The project uses the Oculus SDK to manage VR inputs and Photon Bolt to handle multiplayer networking. We initially integrated Photon Bolt with AWS GameLift, which handled matchmaking and server instance hosting. However, AWS GameLift pricing proved too costly to sustain a live build beyond early development. We instead opted to self-host server instances and create a custom intermediary service that replaces AWS GameLift called Near Orbit Multiplayer Service (NOMS).

The official repository is private. Contact kdliu00@berkeley.edu for inquiries.

Built with [Unity](https://unity.com/).

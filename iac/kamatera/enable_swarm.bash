echo "Initialize docker swarm";
case "$(docker info --format '{{.Swarm.LocalNodeState}}')" in 
  inactive)
    docker swarm init;
    echo "Docker Swarm Node is initializing";;
  pending)
    echo "Docker Swarm Node is not in a swarm cluster";;
  active)
    echo "Docker Swarm Node is in a swarm cluster";;
  locked)
    echo "Docker Swarm Node is in a locked swarm cluster";;
  error)
    echo "Docker Swarm Node is in an error state";;
  *)
    echo "Docker Swarm Node is in an unknown state $(docker info --format '{{.Swarm.LocalNodeState}}')";;
esac
from apscheduler.schedulers.background import BackgroundScheduler
import time

def hello():
    print("hello this time is AM 8:00")

def main():
    timeset = BackgroundScheduler(timezone = 'Asia/Seoul')
    timeset.add_job(hello, 'cron', hour = '8', minute = '00', id = 'alarm')
    timeset.start()

if __name__ == "__main__":
    main()
